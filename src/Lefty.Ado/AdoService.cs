using Lefty.Ado.Model;
using Microsoft.Extensions.Options;

namespace Lefty.Ado;

/// <summary />
public class AdoService
{
    private readonly string _project;


    /// <summary />
    public AdoService( IOptionsSnapshot<AdoServiceOptions> options )
    {
        _project = options.Value.DefaultProject;
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
}