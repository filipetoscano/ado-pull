using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Ado.Cli;

/// <summary />
[Command( "export", Description = "Export all work items into Sqlite database" )]
public class ExportCommand
{
    private readonly IAdoService _ado;


    /// <summary />
    public ExportCommand( IAdoService ado )
    {
        _ado = ado;
    }


    /// <summary />
    [Argument( 0, Description = "Project name" )]
    [Required]
    public string? Project { get; set; }

    /// <summary />
    [Option( "-o|--output-file", CommandOptionType.SingleValue, Description = "Output filename" )]
    public string? OutputFilename { get; set; }


    /// <summary />
    public async Task<int> OnExecuteAsync( CommandLineApplication app, CancellationToken cancellationToken )
    {
        /*
         * Fetch all work items from project
         */
        var items = await _ado.WorkItemListAsync( this.Project!, cancellationToken );


        /*
         * If not specified, infer the filename based on the current date time
         */
        if ( this.OutputFilename == null )
        {
            var now = DateTime.Now;
            this.OutputFilename = $"{this.Project}-{now:yyyyMMdd-HHmm}.db";
        }


        /*
         * 
         */

        // open output
        // run SqliteSchema
        // run DatabaseReset


        // open transaction

        // insert Iteration
        // insert AppUser
        // insert WorkItem
        // insert WorkItemRemark
        // insert WorkItemTransition

        // commit transaction


        return 0;
    }
}