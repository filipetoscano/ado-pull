using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace Lefty.Ado.McpHost.Tools;

/// <summary />
[McpServerToolType]
public class WorkItemRecentlyChangedTool
{
    private readonly AdoService _ado;


    /// <summary />
    public WorkItemRecentlyChangedTool( AdoService ado )
    {
        _ado = ado;
    }


    /// <summary />
    [McpServerTool( Name = "workitem-recently-changed" )]
    [Description( "Lists work items changed within the last N hours." )]
    public async Task<string> WorkItemRecentlyChanged( string project, int hours, CancellationToken cancellationToken )
    {
        var workItems = await _ado.WorkItemRecentlyChangedAsync( project, hours, cancellationToken );


        /*
         *
         */
        var sb = new StringBuilder();
        sb.AppendLine( "| Id | Component | Title | Type | State | Assigned To | Changed | Iteration |" );
        sb.AppendLine( "|----|-----------|-------|------|-------|-------------|---------|-----------|" );

        foreach ( var wi in workItems.OrderByDescending( x => x.MomentActivity ) )
        {
            sb.AppendFormat( "| {0} | {1} | {2} | {3} | {4} | {5} | {6:yyyy-MM-dd HH:mm} | {7} |",
                wi.Id,
                wi.Component,
                wi.Title,
                wi.IssueType,
                wi.State,
                wi.AssignedTo?.DisplayName ?? "",
                wi.MomentActivity,
                wi.Iteration?.Name ?? "" );

            sb.AppendLine();
        }

        return sb.ToString();
    }
}