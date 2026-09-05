using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Spectre.Console;
using System.Reflection;

namespace Lefty.Ado.Cli;

/// <summary />
[Command( "adopull", Description = "Retrieves/Queries items from Azure DevOps" )]
[Subcommand( typeof( ExportCommand ) )]
[VersionOptionFromMember( MemberName = nameof( GetVersion ) )]
public class Program
{
    /// <summary />
    public static int Main( string[] args )
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is( LogEventLevel.Debug )
            .WriteTo.Console(
                standardErrorFromLevel: LogEventLevel.Verbose,
                outputTemplate: "{Level:w3}: {Message:lj}{NewLine}{Exception}" )
            .CreateLogger();

        try
        {
            return Run( args );
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }


    /// <summary />
    private static int Run( string[] args )
    {
        /*
         * 
         */
        var svc = new ServiceCollection();

        svc.AddLogging( b => b.AddSerilog( Log.Logger ) );

        svc.AddOptions<AdoServiceOptions>();
        svc.Configure<AdoServiceOptions>( o =>
        {
            o.Organization = Environment.GetEnvironmentVariable( "ADO_ORG" )!;
            o.DefaultProject = Environment.GetEnvironmentVariable( "ADO_PROJ" )!;
            o.PersonalAccessToken = Environment.GetEnvironmentVariable( "ADO_PAT" )!;
        } );

        svc.AddTransient<IAdoService, AdoService>();
        svc.AddHttpClient<AdoService>();

        var sp = svc.BuildServiceProvider();


        /*
         * 
         */
        var app = new CommandLineApplication<Program>();

        try
        {
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection( sp );
        }
        catch ( Exception ex )
        {
            AnsiConsole.MarkupLine( $"[purple]ftl[/]: unhandled exception" );
            AnsiConsole.WriteException( ex );

            return 2;
        }


        /*
         * 
         */
        try
        {
            return app.Execute( args );
        }
        catch ( CommandParsingException ex )
        {
            /*
             * The base type, not UnrecognizedCommandParsingException: a value that
             * fails to parse into its option's type -- 'format -n abc', or a bogus
             * --format -- raises the base directly, and is bad input just the same,
             * so it earns an error line rather than a stack trace.
             */
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: {ex.Message}" );

            return 2;
        }
        catch ( ThreadAbortException )
        {
            Console.WriteLine( "// cancelled //" );

            return 2;
        }
        catch ( Exception ex )
        {
            AnsiConsole.MarkupLine( $"[purple]ftl[/]: unhandled exception" );
            AnsiConsole.WriteException( ex );

            return 2;
        }
    }


    /// <summary />
    private static string GetVersion()
    {
        return typeof( Program )
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;
    }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        app.ShowHelp();
        return 1;
    }
}