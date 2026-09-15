# ado-pull

Azure DevOps library, api, and mcp host.

## Objective

- Library to query ADO in `Lefty.Ado`
- Models and interface in `Lefty.Ado.Abstractions`
- Web host `Lefty.Ado.McpHost`
   - Exposes library interface as REST API
   - Exposes tools in MCP server (Lefty.Ado.McpHost)
- Has command-line tool in `Lefty.Ado.Cli`


## Validation

- Always run dotnet format (in root)
