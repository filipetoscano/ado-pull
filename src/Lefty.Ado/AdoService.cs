using Lefty.Ado.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Lefty.Ado;

/// <summary />
public partial class AdoService : IAdoService
{
    private const string ApiVersion = "7.1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly ILogger<AdoService> _logger;


    /// <summary />
    public AdoService( HttpClient httpClient, IOptionsSnapshot<AdoServiceOptions> options, ILogger<AdoService> logger )
    {
        var opts = options.Value;

        _logger = logger;

        _http = httpClient;
        _http.BaseAddress = new Uri( $"https://dev.azure.com/{opts.Organization}/" );
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String( Encoding.ASCII.GetBytes( $":{opts.PersonalAccessToken}" ) ) );
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
                Id = node.Identifier,
                Name = node.Name,
                DateStart = node.Attributes?.StartDate is { } s ? DateOnly.FromDateTime( s.UtcDateTime ) : null,
                DateEnd = node.Attributes?.FinishDate is { } f ? DateOnly.FromDateTime( f.UtcDateTime ) : null,
            } );

            FlattenIterations( node.Children, into );
        }
    }
}
