using Lefty.Ado.Model;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace Lefty.Ado.McpHost.Tools;

/// <summary />
[McpServerToolType]
public class WorkItemGetTool
{
    private readonly AdoService _ado;


    /// <summary />
    public WorkItemGetTool( AdoService ado )
    {
        _ado = ado;
    }


    /// <summary />
    [McpServerTool( Name = "workitem-get" )]
    [Description( "Gets a single work item by id." )]
    public async Task<string> WorkItemGet( int id, CancellationToken cancellationToken )
    {
        WorkItem wi;

        try
        {
            wi = await _ado.WorkItemGetAsync( id, cancellationToken );
        }
        catch ( HttpRequestException ex ) when ( ex.StatusCode == HttpStatusCode.NotFound )
        {
            return $"Work item {id} was not found.";
        }


        /*
         *
         */
        var sb = new StringBuilder();
        sb.AppendLine( $"# {wi.Id}: {wi.Title}" );
        sb.AppendLine();
        sb.AppendLine( $"- Type: {wi.IssueType}" );
        sb.AppendLine( $"- State: {wi.State}" );
        sb.AppendLine( $"- Assigned To: {wi.AssignedTo?.DisplayName ?? ""}" );
        sb.AppendLine( $"- Iteration: {wi.Iteration?.Name ?? ""}" );
        sb.AppendLine( $"- Created By: {wi.CreatedBy.DisplayName} ({wi.MomentCreated:yyyy-MM-dd})" );
        sb.AppendLine();
        sb.AppendLine( wi.Description );

        if ( wi.Remarks.Count > 0 )
        {
            sb.AppendLine();
            sb.AppendLine( "## Remarks" );

            foreach ( var r in wi.Remarks.OrderBy( x => x.Moment ) )
                sb.AppendLine( $"- {r.Moment:yyyy-MM-dd} {r.By.DisplayName}: {r.Text}" );
        }

        if ( wi.Transitions.Count > 0 )
        {
            sb.AppendLine();
            sb.AppendLine( "## Transitions" );

            foreach ( var t in wi.Transitions.OrderBy( x => x.Moment ) )
                sb.AppendLine( $"- {t.Moment:yyyy-MM-dd} {t.By.DisplayName}: {t.From} -> {t.To}" );
        }

        return sb.ToString();
    }
}
