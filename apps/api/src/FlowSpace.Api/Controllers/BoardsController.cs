using System.Security.Claims;
using FlowSpace.Application.Boards.Commands.CreateBoard;
using FlowSpace.Application.Boards.Commands.DeleteBoard;
using FlowSpace.Application.Boards.Commands.UpdateBoard;
using FlowSpace.Application.Boards.Commands.UpdateWhiteboard;
using FlowSpace.Application.Boards.Queries.ListBoards;
using FlowSpace.Application.Boards.Queries.GetWhiteboard;
using FlowSpace.Application.Common.Abstractions.Authorization;
using FlowSpace.Contracts.Boards;
using FlowSpace.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSpace.Api.Controllers;

[Route("api/v1/workspaces/{workspaceId:guid}/boards")]
[Authorize]
public class BoardsController : ApiController
{
    private readonly ISender _sender;
    private readonly IPermissionService _permissionService;

    public BoardsController(ISender sender, IPermissionService permissionService)
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

    [HttpPost]
    [Authorize(Policy = Permissions.BoardCreate)]
    public async Task<IActionResult> Create(Guid workspaceId, CreateBoardRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateBoardCommand(workspaceId, request.Name, request.Type, userId);
        var result = await _sender.Send(command);

        return result.IsSuccess ? CreatedAtAction(nameof(List), new { workspaceId }, result.Value) : HandleFailure(result);
    }

    [HttpGet]
    [Authorize(Policy = Permissions.NodeRead)]
    public async Task<IActionResult> List(Guid workspaceId)
    {
        var query = new ListBoardsQuery(workspaceId);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet("{id:guid}/whiteboard")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWhiteboard(Guid workspaceId, Guid id, [FromQuery] string? token)
    {
        if (!await HasPermission(id, Permissions.NodeRead, token))
            return Unauthorized();

        var query = new GetWhiteboardQuery(id);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.BoardUpdate)]
    public async Task<IActionResult> Update(Guid id, UpdateBoardRequest request)
    {
        var command = new UpdateBoardCommand(id, request.Name);
        var result = await _sender.Send(command);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.BoardDelete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteBoardCommand(id);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }

    [HttpPut("{id:guid}/whiteboard")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateWhiteboard(Guid workspaceId, Guid id, [FromQuery] string? token, [FromBody] Dictionary<string, object> records)
    {
        if (!await HasPermission(id, Permissions.BoardUpdate, token))
            return Unauthorized();

        var command = new UpdateWhiteboardCommand(id, records);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}
