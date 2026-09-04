namespace Lefty.Ado.McpHost;

/// <summary />
public class Program
{
    /// <summary />
    public static int Main( string[] args )
    {
        /*
         * 
         */
        var builder = WebApplication.CreateBuilder( args );

        builder.Services.Configure<AdoServiceOptions>( o =>
        {
            o.Organization = Environment.GetEnvironmentVariable( "ADO_ORG" )!;
            o.DefaultProject = Environment.GetEnvironmentVariable( "ADO_PROJ" )!;
            o.PersonalAccessToken = Environment.GetEnvironmentVariable( "ADO_PAT" )!;
        } );

        builder.Services.AddTransient<AdoService>();

        builder.Services.AddControllers();
        builder.Services.AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly();


        /*
         * 
         */
        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.MapMcp();

        app.Run();

        return 0;
    }
}