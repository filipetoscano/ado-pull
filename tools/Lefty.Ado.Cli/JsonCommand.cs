using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using Spectre.Console.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Lefty.Ado.Cli;

/// <summary />
[Command( "json", Description = "Export all work items as JSON file" )]
public class JsonCommand
{
    private readonly IAdoService _ado;


    /// <summary />
    public JsonCommand( IAdoService ado )
    {
        _ado = ado;
    }


    /// <summary />
    [Argument( 0, Description = "Project name" )]
    [Required]
    public string? Project { get; set; }

    /// <summary />
    [Option( "-o|--output-file", CommandOptionType.SingleValue, Description = "Output filename, otherwise console" )]
    public string? OutputFilename { get; set; }


    /// <summary />
    public async Task<int> OnExecuteAsync( CommandLineApplication app, CancellationToken cancellationToken )
    {
        /*
         * Fetch all work items from project
         */
        var items = await _ado.WorkItemListAsync( this.Project!, cancellationToken );


        /*
         * 
         */
        var json = JsonSerializer.Serialize( items, new JsonSerializerOptions()
        {
            WriteIndented = true,
        } );


        /*
         * 
         */
        if ( this.OutputFilename != null )
        {
            await File.WriteAllTextAsync( this.OutputFilename, json, cancellationToken );
        }
        else
        {
            var text = new JsonText( json );
            AnsiConsole.Write( text );
        }

        return 0;
    }
}