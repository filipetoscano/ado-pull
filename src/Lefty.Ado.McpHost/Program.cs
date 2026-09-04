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

        builder.Services.AddOptions();

        builder.Services.AddOptions<AdoServiceOptions>();
        builder.Services.Configure<AdoServiceOptions>( o =>
        {
            o.Organization = Environment.GetEnvironmentVariable( "ADO_ORG" )!;
            o.DefaultProject = Environment.GetEnvironmentVariable( "ADO_PROJ" )!;
            o.PersonalAccessToken = Environment.GetEnvironmentVariable( "ADO_PAT" )!;
        } );

        builder.Services.AddHttpClient<AdoService>();

        builder.Services.AddControllers();
        builder.Services.AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly();

        builder.Services.AddCors( options =>
        {
            options.AddPolicy( "McpInspector", policy =>
            {
                policy
                    .WithOrigins( "http://localhost:6274" )
                    .WithMethods( "POST", "GET", "DELETE" )
                    .WithHeaders(
                        "Content-Type",
                        "Authorization",
                        "MCP-Protocol-Version",
                        "Mcp-Session-Id" )
                    .WithExposedHeaders( "Mcp-Session-Id" );
            } );
        } );


        /*
         * 
         */
        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseCors();

        app.UseAuthorization();
        app.MapControllers();
        app.MapMcp( "/mcp" )
           .RequireCors( "McpInspector" );

        app.Run();

        return 0;
    }
}