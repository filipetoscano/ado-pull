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
    public async Task<IEnumerable<Iteration>> IterationsList()
    {
        var resp = await _svc.IterationListAsync();

        return resp.OrderBy( x => x.Name );
    }


    /// <summary />
    [HttpGet]
    [Route( "/api/projects/{project}/iterations" )]
    public async Task<IEnumerable<Iteration>> IterationsList(
        [FromRoute( Name = "project" )] string project
    )
    {
        var resp = await _svc.IterationListAsync( project );

        return resp.OrderBy( x => x.Name );
    }


    /// <summary />
    [HttpGet]
    [Route( "/api/project/workitems" )]
    public async Task<IReadOnlyList<WorkItem>> WorkItemsList()
    {
        var resp = await _svc.WorkItemListAsync();

        return resp;
    }


    /// <summary />
    [HttpGet]
    [Route( "/api/projects/{project}/workitems" )]
    public async Task<IReadOnlyList<WorkItem>> WorkItemsList(
        [FromRoute( Name = "project" )] string project
    )
    {
        var resp = await _svc.WorkItemListAsync( project );

        return resp;
    }
}