using FlowSpace.Application.Sharing.Commands.CreateShareLink;
using FlowSpace.Application.Sharing.Commands.RevokeShareLink;
using FlowSpace.Application.Sharing.Commands.UpdateBoardVisibility;
using FlowSpace.Application.Sharing.Queries.GetBoardSharingInfo;
using FlowSpace.Application.Sharing.Queries.GetSharedBoard;
using FlowSpace.Contracts.Sharing;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSpace.Api.Controllers;

[Route("api/v1/boards/{boardId:guid}/sharing")]
[Authorize]
public class SharingController : ApiController
{
    private readonly ISender _sender;

    public SharingController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.BoardShare)]
    public async Task<IActionResult> GetSharingInfo(Guid boardId)
    {
        var query = new GetBoardSharingInfoQuery(boardId);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPut("visibility")]
    [Authorize(Policy = Permissions.BoardShare)]
    public async Task<IActionResult> UpdateVisibility(Guid boardId, UpdateBoardVisibilityRequest request)
    {
        var command = new UpdateBoardVisibilityCommand(boardId, (BoardVisibility)request.Visibility);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }

    [HttpPost("links")]
    [Authorize(Policy = Permissions.BoardShare)]
    public async Task<IActionResult> CreateShareLink(Guid boardId, CreateShareLinkRequest request)
    {
        var command = new CreateShareLinkCommand(boardId, (BoardRole)request.Role, request.ExpiresAt);
        var result = await _sender.Send(command);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpDelete("links/{linkId:guid}")]
    [Authorize(Policy = Permissions.BoardShare)]
    public async Task<IActionResult> RevokeShareLink(Guid boardId, Guid linkId)
    {
        var command = new RevokeShareLinkCommand(linkId);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }

    [HttpGet("/api/v1/shared/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSharedBoard(string token)
    {
        var query = new GetSharedBoardQuery(token);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}
