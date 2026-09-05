using Lefty.Ado.Model;
using System.Net.Http.Json;

namespace Lefty.Ado;

public partial class AdoService
{
    /// <summary />
    public async Task<IReadOnlyList<TestPlan>> TestPlanListAsync( string project, CancellationToken cancellationToken = default )
    {
        var plans = new List<TestPlan>();
        string? continuationToken = null;

        do
        {
            var url = $"{Uri.EscapeDataString( project )}/_apis/testplan/plans?includePlanDetails=true&api-version={ApiVersion}"
                + ( continuationToken is not null ? $"&continuationToken={Uri.EscapeDataString( continuationToken )}" : "" );

            using var resp = await _http.GetAsync( url, cancellationToken );
            resp.EnsureSuccessStatusCode();

            var result = await resp.Content.ReadFromJsonAsync<TestPlanListResultDto>( JsonOptions, cancellationToken )
                ?? throw new InvalidOperationException( "Test plan list response was empty." );

            plans.AddRange( result.Value.Select( MapTestPlan ) );

            continuationToken = resp.Headers.TryGetValues( "x-ms-continuationtoken", out var values )
                ? values.FirstOrDefault()
                : null;
        }
        while ( continuationToken is not null );

        await FillSuitesAsync( project, plans, cancellationToken );

        return plans;
    }


    /// <summary />
    private static TestPlan MapTestPlan( TestPlanDto dto )
    {
        return new TestPlan
        {
            Id = dto.Id,
            Name = dto.Name,
            AreaPath = dto.AreaPath,
            Iteration = dto.Iteration,
            State = dto.State,
            DateStart = dto.StartDate is { } s ? DateOnly.FromDateTime( s.UtcDateTime ) : null,
            DateEnd = dto.EndDate is { } e ? DateOnly.FromDateTime( e.UtcDateTime ) : null,
            Owner = dto.Owner is { } o
                ? new User { Id = o.Id, DisplayName = o.DisplayName ?? "", Upn = o.UniqueName ?? "" }
                : throw new InvalidOperationException( $"Test plan {dto.Id} has no owner." ),
            Suites = Array.Empty<TestSuite>(),
        };
    }


    /// <summary>
    /// One extra call per plan (no bulk "suites for many plans" endpoint exists),
    /// same bounded-concurrency shape as FillTransitionsAndRemarksAsync.
    /// </summary>
    private async Task FillSuitesAsync( string project, IReadOnlyList<TestPlan> plans, CancellationToken cancellationToken )
    {
        using var throttle = new SemaphoreSlim( 8 );

        await Task.WhenAll( plans.Select( async plan =>
        {
            await throttle.WaitAsync( cancellationToken );

            try
            {
                plan.Suites = await TestSuiteListAsync( project, plan.Id, cancellationToken );
            }
            finally
            {
                throttle.Release();
            }
        } ) );
    }


    /// <summary />
    public async Task<IReadOnlyList<TestSuite>> TestSuiteListAsync( string project, int planId, CancellationToken cancellationToken = default )
    {
        var suites = new List<TestSuite>();
        string? continuationToken = null;

        do
        {
            var url = $"{Uri.EscapeDataString( project )}/_apis/testplan/Plans/{planId}/suites?api-version={ApiVersion}"
                + ( continuationToken is not null ? $"&continuationToken={Uri.EscapeDataString( continuationToken )}" : "" );

            using var resp = await _http.GetAsync( url, cancellationToken );
            resp.EnsureSuccessStatusCode();

            var result = await resp.Content.ReadFromJsonAsync<TestSuiteListResultDto>( JsonOptions, cancellationToken )
                ?? throw new InvalidOperationException( "Test suite list response was empty." );

            suites.AddRange( result.Value.Select( MapTestSuite ) );

            continuationToken = resp.Headers.TryGetValues( "x-ms-continuationtoken", out var values )
                ? values.FirstOrDefault()
                : null;
        }
        while ( continuationToken is not null );

        await FillTestCasesAsync( project, planId, suites, cancellationToken );

        return suites;
    }


    /// <summary />
    private static TestSuite MapTestSuite( TestSuiteDto dto )
    {
        return new TestSuite
        {
            Id = dto.Id,
            Name = dto.Name,
            SuiteType = dto.SuiteType,
            ParentSuiteId = dto.ParentSuite?.Id,
            TestCases = Array.Empty<TestCase>(),
        };
    }


    /// <summary>
    /// One extra call per suite (no bulk "test cases for many suites" endpoint
    /// exists), same bounded-concurrency shape as FillTransitionsAndRemarksAsync.
    /// </summary>
    private async Task FillTestCasesAsync( string project, int planId, IReadOnlyList<TestSuite> suites, CancellationToken cancellationToken )
    {
        using var throttle = new SemaphoreSlim( 8 );

        await Task.WhenAll( suites.Select( async suite =>
        {
            await throttle.WaitAsync( cancellationToken );

            try
            {
                suite.TestCases = await FetchTestCasesAsync( project, planId, suite.Id, cancellationToken );
            }
            finally
            {
                throttle.Release();
            }
        } ) );
    }


    /// <summary>
    /// Test cases for a suite come from the point list -- one row per (test
    /// case, configuration) -- which also carries each point's current
    /// outcome and last run/result reference "for free". Paginated via
    /// $skip/$top rather than a continuation-token header, unlike every
    /// other list call in this file.
    /// </summary>
    private async Task<IReadOnlyList<TestCase>> FetchTestCasesAsync( string project, int planId, int suiteId, CancellationToken cancellationToken )
    {
        const int pageSize = 200;
        var points = new List<TestPointDto>();
        var skip = 0;

        while ( true )
        {
            var url = $"{Uri.EscapeDataString( project )}/_apis/test/Plans/{planId}/Suites/{suiteId}/points"
                + $"?includePointDetails=true&witFields=System.Title&$skip={skip}&$top={pageSize}&api-version={ApiVersion}";

            var result = await _http.GetFromJsonAsync<TestPointListResultDto>( url, JsonOptions, cancellationToken )
                ?? throw new InvalidOperationException( "Test point list response was empty." );

            points.AddRange( result.Value );

            if ( result.Value.Count < pageSize )
                break;

            skip += pageSize;
        }

        return points
            .GroupBy( p => p.TestCase.Id )
            .Select( g => MapTestCase( g.Key, g.ToList() ) )
            .ToList();
    }


    /// <summary />
    private static TestCase MapTestCase( string testCaseId, List<TestPointDto> points )
    {
        var title = points
            .SelectMany( p => p.WorkItemProperties ?? Enumerable.Empty<WorkItemPropertyDto>() )
            .Select( p => p.WorkItem )
            .FirstOrDefault( w => w?.Key == "System.Title" )
            ?.Value ?? "";

        return new TestCase
        {
            WorkItemId = int.Parse( testCaseId ),
            Title = title,
            Points = points.Select( MapTestPoint ).ToList(),
        };
    }


    /// <summary />
    private static TestPoint MapTestPoint( TestPointDto dto )
    {
        return new TestPoint
        {
            Id = dto.Id,
            ConfigurationName = dto.Configuration?.Name ?? "",
            Tester = dto.AssignedTo is { } a
                ? new User { Id = a.Id, DisplayName = a.DisplayName ?? "", Upn = a.UniqueName ?? "" }
                : null,
            Outcome = dto.Outcome,
            LastRunId = dto.LastTestRun?.Id is { } r && int.TryParse( r, out var runId ) ? runId : null,
            LastResultId = dto.LastResult?.Id is { } rid && int.TryParse( rid, out var resultId ) ? resultId : null,
        };
    }
}