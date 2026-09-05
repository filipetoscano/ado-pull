using Lefty.Ado.Model;

namespace Lefty.Ado;

/// <summary />
public interface IAdoService
{
    /// <summary />
    Task<IReadOnlyList<Iteration>> IterationListAsync( string project, CancellationToken cancellationToken = default );

    /// <summary />
    Task<IReadOnlyList<WorkItem>> WorkItemListAsync( string project, CancellationToken cancellationToken = default );

    /// <summary />
    Task<IReadOnlyList<WorkItem>> WorkItemRecentlyChangedAsync( string project, int hourWindow, CancellationToken cancellationToken = default );

    /// <summary />
    Task<WorkItem> WorkItemGetAsync( string project, int id, CancellationToken cancellationToken );
}