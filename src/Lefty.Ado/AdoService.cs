using Lefty.Ado.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Lefty.Ado;

/// <summary />
public class AdoService
{
    private const string ApiVersion = "7.1";
    private const string CommentsApiVersion = "7.1-preview.4";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly string[] WorkItemFields =
    {
        "System.Title",
        "System.Description",
        "System.State",
        "System.CreatedBy",
        "System.CreatedDate",
        "System.ChangedDate",
        "System.AssignedTo",
        "System.Tags",
        "System.IterationPath",
        "System.WorkItemType",
        "Microsoft.VSTS.Common.Severity",
    };

    private readonly HttpClient _http;
    private readonly string _project;
    private readonly ILogger<AdoService> _logger;


    /// <summary />
    public AdoService( HttpClient httpClient, IOptionsSnapshot<AdoServiceOptions> options, ILogger<AdoService> logger )
    {
        var opts = options.Value;

        _project = opts.DefaultProject;
        _logger = logger;

        _http = httpClient;
        _http.BaseAddress = new Uri( $"https://dev.azure.com/{opts.Organization}/" );
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String( Encoding.ASCII.GetBytes( $":{opts.PersonalAccessToken}" ) ) );
    }


    /// <summary />
    public Task<IReadOnlyList<Iteration>> IterationListAsync( CancellationToken cancellationToken = default )
    {
        return IterationListAsync( _project, cancellationToken );
    }


    /// <summary />
    public async Task<IReadOnlyList<Iteration>> IterationListAsync( string project, CancellationToken cancellationToken = default )
    {
        var url = $"{Uri.EscapeDataString( project )}/_apis/wit/classificationnodes/Iterations?$depth=10&api-version={ApiVersion}";

        var root = await _http.GetFromJsonAsync<ClassificationNodeDto>( url, JsonOptions, cancellationToken )
            ?? throw new InvalidOperationException( "Iteration classification tree was not found." );

        var iterations = new List<Iteration>();
        FlattenIterations( root.Children, iterations );

        return iterations;
    }


    /// <summary />
    private static void FlattenIterations( List<ClassificationNodeDto>? nodes, List<Iteration> into )
    {
        if ( nodes is null )
            return;

        foreach ( var node in nodes )
        {
            into.Add( new Iteration
            {
                Name = node.Name,
                DateStart = node.Attributes?.StartDate is { } s ? DateOnly.FromDateTime( s.UtcDateTime ) : null,
                DateEnd = node.Attributes?.FinishDate is { } f ? DateOnly.FromDateTime( f.UtcDateTime ) : null,
            } );

            FlattenIterations( node.Children, into );
        }
    }


    /// <summary />
    public Task<IReadOnlyList<WorkItem>> WorkItemListAsync( CancellationToken cancellationToken = default )
    {
        return WorkItemListAsync( _project, cancellationToken );
    }


    /// <summary>
    /// Lists work items in <paramref name="project" /> that are not in a
    /// terminal state ('Closed', 'Removed' or 'Done').
    /// </summary>
    public async Task<IReadOnlyList<WorkItem>> WorkItemListAsync( string project, CancellationToken cancellationToken = default )
    {
        var ids = await QueryActiveWorkItemIdsAsync( project, cancellationToken );

        if ( ids.Count == 0 )
            return Array.Empty<WorkItem>();

        var iterations = await IterationListAsync( project, cancellationToken );
        var iterationsByName = iterations
            .GroupBy( x => x.Name, StringComparer.OrdinalIgnoreCase )
            .ToDictionary( g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase );

        var items = new List<WorkItem>();

        foreach ( var chunk in ids.Chunk( 200 ) )
        {
            items.AddRange( await FetchWorkItemBatchAsync( project, chunk, iterationsByName, cancellationToken ) );
        }


        /*
         * Transitions and remarks require one extra call per work item
         * (no batch API exists for either), so fan them out with bounded
         * concurrency rather than one at a time.
         */
        using var throttle = new SemaphoreSlim( 8 );

        await Task.WhenAll( items.Select( async item =>
        {
            await throttle.WaitAsync( cancellationToken );

            try
            {
                item.Transitions = await FetchTransitionsAsync( project, item.Id, cancellationToken );
                item.Remarks = await FetchRemarksAsync( project, item.Id, cancellationToken );
            }
            finally
            {
                throttle.Release();
            }
        } ) );

        return items;
    }


    /// <summary />
    private async Task<List<int>> QueryActiveWorkItemIdsAsync( string project, CancellationToken cancellationToken )
    {
        var url = $"{Uri.EscapeDataString( project )}/_apis/wit/wiql?api-version={ApiVersion}";

        var query =
            "SELECT [System.Id] FROM WorkItems " +
            "WHERE [System.TeamProject] = @project " +
            "AND [System.State] NOT IN ('Closed', 'Removed', 'Done') " +
            "ORDER BY [System.ChangedDate] DESC";

        var resp = await _http.PostAsJsonAsync( url, new { query }, JsonOptions, cancellationToken );
        resp.EnsureSuccessStatusCode();

        var result = await resp.Content.ReadFromJsonAsync<WiqlResultDto>( JsonOptions, cancellationToken )
            ?? throw new InvalidOperationException( "WIQL response was empty." );

        return result.WorkItems.Select( x => x.Id ).ToList();
    }


    /// <summary />
    private async Task<List<WorkItem>> FetchWorkItemBatchAsync(
        string project,
        IReadOnlyCollection<int> ids,
        IReadOnlyDictionary<string, Iteration> iterationsByName,
        CancellationToken cancellationToken )
    {
        var url = $"{Uri.EscapeDataString( project )}/_apis/wit/workitemsbatch?api-version={ApiVersion}";

        var resp = await _http.PostAsJsonAsync( url, new { ids, fields = WorkItemFields }, JsonOptions, cancellationToken );
        resp.EnsureSuccessStatusCode();

        var result = await resp.Content.ReadFromJsonAsync<WorkItemBatchResultDto>( JsonOptions, cancellationToken )
            ?? throw new InvalidOperationException( "Work item batch response was empty." );

        return result.Value.Select( dto => MapWorkItem( dto, iterationsByName ) ).ToList();
    }


    /// <summary />
    private static WorkItem MapWorkItem( WorkItemBatchItemDto dto, IReadOnlyDictionary<string, Iteration> iterationsByName )
    {
        var fields = dto.Fields;

        var iterationPath = GetString( fields, "System.IterationPath" );
        Iteration? iteration = null;

        if ( iterationPath is not null )
        {
            var iterationName = iterationPath.Split( '\\' ).Last();

            iteration = iterationsByName.TryGetValue( iterationName, out var found )
                ? found
                : new Iteration { Name = iterationName };
        }

        var tags = GetString( fields, "System.Tags" );

        return new WorkItem
        {
            Id = dto.Id,
            Title = GetString( fields, "System.Title" ) ?? "",
            Description = GetString( fields, "System.Description" ) ?? "",
            State = GetString( fields, "System.State" ) ?? "",
            CreatedBy = GetUser( fields, "System.CreatedBy" )
                ?? throw new InvalidOperationException( $"Work item {dto.Id} has no System.CreatedBy." ),
            MomentCreated = GetDateTime( fields, "System.CreatedDate" )
                ?? throw new InvalidOperationException( $"Work item {dto.Id} has no System.CreatedDate." ),
            MomentActivity = GetDateTime( fields, "System.ChangedDate" )
                ?? throw new InvalidOperationException( $"Work item {dto.Id} has no System.ChangedDate." ),
            AssignedTo = GetUser( fields, "System.AssignedTo" ),
            Tags = string.IsNullOrEmpty( tags )
                ? Array.Empty<string>()
                : tags.Split( ';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries ),
            Iteration = iteration,
            IssueType = GetString( fields, "System.WorkItemType" ),
            Component = null,
            Severity = GetString( fields, "Microsoft.VSTS.Common.Severity" ),
            Transitions = Array.Empty<WorkItemTransition>(),
            Remarks = Array.Empty<WorkItemRemark>(),
        };
    }


    /// <summary />
    private async Task<IReadOnlyList<WorkItemTransition>> FetchTransitionsAsync( string project, int id, CancellationToken cancellationToken )
    {
        var url = $"{Uri.EscapeDataString( project )}/_apis/wit/workitems/{id}/updates?api-version={ApiVersion}";

        var result = await _http.GetFromJsonAsync<WorkItemUpdatesResultDto>( url, JsonOptions, cancellationToken );

        if ( result is null )
            return Array.Empty<WorkItemTransition>();

        var transitions = new List<WorkItemTransition>();

        foreach ( var update in result.Value )
        {
            if ( update.Fields is null || update.RevisedBy is null || update.RevisedDate is null )
                continue;

            if ( !update.Fields.TryGetValue( "System.State", out var change ) )
                continue;

            var from = change.OldValue is { ValueKind: JsonValueKind.String } ov ? ov.GetString() : null;
            var to = change.NewValue is { ValueKind: JsonValueKind.String } nv ? nv.GetString() : null;

            // A missing 'from' means this update is the work item's creation, not a transition.
            if ( from is null || to is null )
                continue;

            transitions.Add( new WorkItemTransition
            {
                From = from,
                To = to,
                By = new User { DisplayName = update.RevisedBy.DisplayName ?? "", Upn = update.RevisedBy.UniqueName ?? "" },
                Moment = update.RevisedDate.Value.UtcDateTime,
            } );
        }

        return transitions;
    }


    /// <summary />
    private async Task<IReadOnlyList<WorkItemRemark>> FetchRemarksAsync( string project, int id, CancellationToken cancellationToken )
    {
        var url = $"{Uri.EscapeDataString( project )}/_apis/wit/workitems/{id}/comments?api-version={CommentsApiVersion}";

        var result = await _http.GetFromJsonAsync<WorkItemCommentsResultDto>( url, JsonOptions, cancellationToken );

        if ( result is null )
            return Array.Empty<WorkItemRemark>();

        return result.Comments
            .Where( c => c.CreatedBy is not null && c.CreatedDate is not null )
            .Select( c => new WorkItemRemark
            {
                Text = c.Text,
                By = new User { DisplayName = c.CreatedBy!.DisplayName ?? "", Upn = c.CreatedBy!.UniqueName ?? "" },
                Moment = c.CreatedDate!.Value.UtcDateTime,
            } )
            .ToList();
    }


    /// <summary />
    private static string? GetString( Dictionary<string, JsonElement> fields, string name )
    {
        return fields.TryGetValue( name, out var el ) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;
    }


    /// <summary />
    private static DateTime? GetDateTime( Dictionary<string, JsonElement> fields, string name )
    {
        return fields.TryGetValue( name, out var el ) && el.ValueKind == JsonValueKind.String
            ? el.GetDateTime()
            : null;
    }


    /// <summary />
    private static User? GetUser( Dictionary<string, JsonElement> fields, string name )
    {
        if ( !fields.TryGetValue( name, out var el ) || el.ValueKind != JsonValueKind.Object )
            return null;

        var displayName = el.TryGetProperty( "displayName", out var dn ) ? dn.GetString() : null;
        var uniqueName = el.TryGetProperty( "uniqueName", out var un ) ? un.GetString() : null;

        if ( displayName is null || uniqueName is null )
            return null;

        return new User { DisplayName = displayName, Upn = uniqueName };
    }
}
