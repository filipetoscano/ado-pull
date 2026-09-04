using Lefty.Ado.Model;
using Microsoft.AspNetCore.Mvc;

namespace Lefty.Ado.McpHost.Controllers;

/// <summary />
[ApiController]
public class AdoController : ControllerBase
{
    private readonly AdoService _svc;


    /// <summary />
    public AdoController( AdoService svc )
    {
        _svc = svc;
    }


    /// <summary />
    [HttpGet]
    [Route( "/api/project/iterations" )]
    public async Task<IReadOnlyList<Iteration>> IterationsList()
    {
        var resp = await _svc.IterationListAsync();

        return resp;
    }


    /// <summary />
    [HttpGet]
    [Route( "/api/projects/{project}/iterations" )]
    public async Task<IReadOnlyList<Iteration>> IterationsList(
        [FromRoute( Name = "project" )] string project
    )
    {
        var resp = await _svc.IterationListAsync( project );

        return resp;
    }
}