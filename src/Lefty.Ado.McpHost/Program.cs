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
                    .WithOrigins( "http://localhost:6275" )
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


        /*
         * Skip HTTPS redirection in Development: tools like the MCP Inspector
         * run their proxy on Node, which validates TLS against its own CA
         * bundle rather than the OS store, so it rejects Kestrel's self-signed
         * dev certificate even when the OS/.NET trust it. Redirecting an HTTP
         * hit back to HTTPS would just bounce those clients into that failure,
         * so plain HTTP is left reachable directly for local dev.
         */
        if ( app.Environment.IsDevelopment() == false )
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