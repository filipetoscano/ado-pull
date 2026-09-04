using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Lefty.Ado.McpHost.Tools;

/// <summary />
[McpServerToolType]
public static class EchoTool
{
    /// <summary />
    [McpServerTool( Name = "echo" )]
    [Description( "Echoes the message back to the client." )]
    public static string Echo( string message )
    {
        return $"hello {message}";
    }
}