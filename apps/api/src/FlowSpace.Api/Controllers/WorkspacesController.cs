using System.Security.Claims;
using FlowSpace.Application.Workspaces.Commands.CreateWorkspace;
using FlowSpace.Application.Workspaces.Commands.DeleteWorkspace;
using FlowSpace.Application.Workspaces.Commands.UpdateWorkspace;
using FlowSpace.Application.Workspaces.Queries.GetWorkspace;
using FlowSpace.Application.Workspaces.Queries.ListWorkspaces;
using FlowSpace.Contracts.Workspaces;
using FlowSpace.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSpace.Api.Controllers;

[Route("api/v1/workspaces")]
[Authorize]
public class WorkspacesController : ApiController
{
    private readonly ISender _sender;

    public WorkspacesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkspaceRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateWorkspaceCommand(request.Name, userId);
        var result = await _sender.Send(command);

        return result.IsSuccess ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value) : HandleFailure(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.NodeRead)]
    public async Task<IActionResult> Get(Guid id)
    {
        var query = new GetWorkspaceQuery(id);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new ListWorkspacesQuery(userId);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.WorkspaceUpdate)]
    public async Task<IActionResult> Update(Guid id, UpdateWorkspaceRequest request)
    {
        var command = new UpdateWorkspaceCommand(id, request.Name);
        var result = await _sender.Send(command);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.WorkspaceDelete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteWorkspaceCommand(id);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}
