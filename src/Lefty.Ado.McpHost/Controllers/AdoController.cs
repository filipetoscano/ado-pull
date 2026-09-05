using Lefty.Ado.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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


    /// <summary />
    [HttpGet]
    [Route( "/api/project/workitems/recent" )]
    public async Task<IReadOnlyList<WorkItem>> WorkItemsRecentlyChanged(
        [FromQuery( Name = "hours" )] int hours,
        CancellationToken cancellationToken
    )
    {
        var resp = await _svc.WorkItemRecentlyChangedAsync( hours, cancellationToken );

        return resp;
    }


    /// <summary />
    [HttpGet]
    [Route( "/api/projects/{project}/workitems/recent" )]
    public async Task<IReadOnlyList<WorkItem>> WorkItemsRecentlyChanged(
        [FromRoute( Name = "project" )] string project,
        [FromQuery( Name = "hours" )] int hours,
        CancellationToken cancellationToken
    )
    {
        var resp = await _svc.WorkItemRecentlyChangedAsync( project, hours, cancellationToken );

        return resp;
    }


    /// <summary />
    [HttpGet]
    [Route( "/api/project/workitems/{id}" )]
    public async Task<ActionResult<WorkItem>> WorkItemGet(
        [FromRoute( Name = "id" )] int id,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await _svc.WorkItemGetAsync( id, cancellationToken );
        }
        catch ( HttpRequestException ex ) when ( ex.StatusCode == HttpStatusCode.NotFound )
        {
            return NotFound();
        }
    }


    /// <summary />
    [HttpGet]
    [Route( "/api/projects/{project}/workitems/{id}" )]
    public async Task<ActionResult<WorkItem>> WorkItemGet(
        [FromRoute( Name = "project" )] string project,
        [FromRoute( Name = "id" )] int id,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await _svc.WorkItemGetAsync( project, id, cancellationToken );
        }
        catch ( HttpRequestException ex ) when ( ex.StatusCode == HttpStatusCode.NotFound )
        {
            return NotFound();
        }
    }
}