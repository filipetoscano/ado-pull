using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace Lefty.Ado.McpHost.Tools;

/// <summary />
[McpServerToolType]
public class IterationListTool
{
    private readonly AdoService _ado;


    /// <summary />
    public IterationListTool( AdoService ado )
    {
        _ado = ado;
    }


    /// <summary />
    [ McpServerTool( Name = "iteration-list" )]
    [Description( "Lists iterations." )]
    public async Task<string> IterationList()
    {
        var iterations = await _ado.IterationListAsync();


        /*
         * 
         */
        var sb = new StringBuilder();
        sb.AppendLine( "| Iteration | Start | End |" );
        sb.AppendLine( "|-----------|-------|-----|" );

        foreach ( var i in iterations.OrderByDescending( x => x.DateEnd ) )
        { 
            sb.AppendFormat( "| {0} | {1} | {2} |",
                i.Name,
                i.DateStart?.ToString( "yyyy-MM-dd" ) ?? "",
                i.DateEnd?.ToString( "yyyy-MM-dd" ) ?? "" );

            sb.AppendLine();
        }

        return sb.ToString();
    }
}