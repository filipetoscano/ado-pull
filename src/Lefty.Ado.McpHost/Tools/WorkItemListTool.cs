using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace Lefty.Ado.McpHost.Tools;

/// <summary />
[McpServerToolType]
public class WorkItemListTool
{
    private readonly AdoService _ado;


    /// <summary />
    public WorkItemListTool( AdoService ado )
    {
        _ado = ado;
    }


    /// <summary />
    [McpServerTool( Name = "workitem-list" )]
    [Description( "Lists active (non-closed) work items." )]
    public async Task<string> WorkItemList( string project )
    {
        var workItems = await _ado.WorkItemListAsync( project );


        /*
         *
         */
        var sb = new StringBuilder();
        sb.AppendLine( "| Id | Title | Type | State | Assigned To | Iteration |" );
        sb.AppendLine( "|----|-------|------|-------|-------------|-----------|" );

        foreach ( var wi in workItems.OrderBy( x => x.Id ) )
        {
            sb.AppendFormat( "| {0} | {1} | {2} | {3} | {4} | {5} |",
                wi.Id,
                wi.Title,
                wi.IssueType,
                wi.State,
                wi.AssignedTo?.DisplayName ?? "",
                wi.Iteration?.Name ?? "" );

            sb.AppendLine();
        }

        return sb.ToString();
    }
}