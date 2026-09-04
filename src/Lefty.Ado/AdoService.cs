using Lefty.Ado.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lefty.Ado;

/// <summary />
public class AdoService
{
    private readonly string _project;
    private readonly ILogger<AdoService> _logger;


    /// <summary />
    public AdoService( IOptionsSnapshot<AdoServiceOptions> options, ILogger<AdoService> logger )
    {
        _project = options.Value.DefaultProject;
        _logger = logger;
    }


    /// <summary />
    public Task<IReadOnlyList<Iteration>> IterationListAsync( CancellationToken cancellationToken = default )
    {
        return IterationListAsync( _project, cancellationToken );
    }


    /// <summary />
    public async Task<IReadOnlyList<Iteration>> IterationListAsync( string project, CancellationToken cancellationToken = default )
    {
        throw new NotImplementedException();
    }


    /// <summary />
    public Task<IReadOnlyList<WorkItem>> WorkItemListAsync( CancellationToken cancellationToken = default )
    {
        return WorkItemListAsync( _project, cancellationToken );
    }


    /// <summary />
    public async Task<IReadOnlyList<WorkItem>> WorkItemListAsync( string project, CancellationToken cancellationToken = default )
    {
        throw new NotImplementedException();
    }
}