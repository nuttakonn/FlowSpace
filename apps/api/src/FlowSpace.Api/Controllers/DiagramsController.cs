using System.Security.Claims;
using FlowSpace.Application.Diagrams.Commands.CreateEdge;
using FlowSpace.Application.Diagrams.Commands.CreateNode;
using FlowSpace.Application.Diagrams.Commands.DeleteEdge;
using FlowSpace.Application.Diagrams.Commands.DeleteNode;
using FlowSpace.Application.Diagrams.Commands.UpdateNode;
using FlowSpace.Application.Diagrams.Queries.GetElements;
using FlowSpace.Application.Common.Abstractions.Authorization;
using FlowSpace.Contracts.Diagrams;
using FlowSpace.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSpace.Api.Controllers;

[Route("api/v1/boards/{boardId:guid}/elements")]
[Authorize]
public class DiagramsController : ApiController
{
    private readonly ISender _sender;
    private readonly IPermissionService _permissionService;

    public DiagramsController(ISender sender, IPermissionService permissionService)
    {
        _sender = sender;
        _permissionService = permissionService;
    }

    private async Task<bool> HasPermission(Guid boardId, string permission, string? token)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString != null && Guid.TryParse(userIdString, out var userId))
        {
            if (await _permissionService.HasPermissionAsync(userId, permission, boardId))
                return true;
        }

        if (!string.IsNullOrEmpty(token))
        {
            return await _permissionService.HasPermissionAsync(token, permission, boardId);
        }

        return false;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetElements(Guid boardId, [FromQuery] string? token, [FromQuery] double? x, [FromQuery] double? y, [FromQuery] double? width, [FromQuery] double? height)
    {
        if (!await HasPermission(boardId, Permissions.NodeRead, token))
            return Unauthorized();

        var query = new GetElementsQuery(boardId, x, y, width, height);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPost("nodes")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateNode(Guid boardId, [FromQuery] string? token, CreateNodeRequest request)
    {
        if (!await HasPermission(boardId, Permissions.NodeCreate, token))
            return Unauthorized();

        var command = new CreateNodeCommand(
            boardId,
            request.Type,
            request.X,
            request.Y,
            request.Width,
            request.Height,
            request.Metadata);
        
        var result = await _sender.Send(command);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPut("nodes/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateNode(Guid boardId, Guid id, [FromQuery] string? token, UpdateNodeRequest request)
    {
        if (!await HasPermission(boardId, Permissions.NodeUpdate, token))
            return Unauthorized();

        var command = new UpdateNodeCommand(
            id,
            request.X,
            request.Y,
            request.Width,
            request.Height,
            request.Metadata);

        var result = await _sender.Send(command);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpDelete("nodes/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteNode(Guid boardId, Guid id, [FromQuery] string? token)
    {
        if (!await HasPermission(boardId, Permissions.NodeDelete, token))
            return Unauthorized();

        var command = new DeleteNodeCommand(id);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }

    [HttpPost("edges")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateEdge(Guid boardId, [FromQuery] string? token, CreateEdgeRequest request)
    {
        if (!await HasPermission(boardId, Permissions.NodeCreate, token))
            return Unauthorized();

        var command = new CreateEdgeCommand(
            boardId,
            request.SourceNodeId,
            request.TargetNodeId,
            request.Metadata);

        var result = await _sender.Send(command);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpDelete("edges/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteEdge(Guid boardId, Guid id, [FromQuery] string? token)
    {
        if (!await HasPermission(boardId, Permissions.NodeDelete, token))
            return Unauthorized();

        var command = new DeleteEdgeCommand(id);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}
