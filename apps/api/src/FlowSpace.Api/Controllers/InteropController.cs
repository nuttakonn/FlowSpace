using System.Security.Claims;
using FlowSpace.Application.Common.Abstractions.Authorization;
using FlowSpace.Application.Interop.Commands.ExportBoard;
using FlowSpace.Application.Interop.Commands.ImportBoard;
using FlowSpace.Contracts.Interop;
using FlowSpace.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSpace.Api.Controllers;

[Route("api/v1/interop")]
[Authorize]
public class InteropController : ApiController
{
    private readonly ISender _sender;
    private readonly IPermissionService _permissionService;

    public InteropController(ISender sender, IPermissionService permissionService)
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

    [HttpGet("boards/{boardId:guid}/export")]
    [AllowAnonymous]
    public async Task<IActionResult> Export(Guid boardId, [FromQuery] string format, [FromQuery] string? token)
    {
        if (!await HasPermission(boardId, Permissions.NodeRead, token))
            return Unauthorized();

        string authHeader = Request.Headers["Authorization"].ToString();
        string jwtToken = "";
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            jwtToken = authHeader.Substring("Bearer ".Length).Trim();
        }

        string frontendBaseUrl = "";
        string referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            try
            {
                var uri = new Uri(referer);
                frontendBaseUrl = uri.GetLeftPart(UriPartial.Authority);
            }
            catch {}
        }

        var command = new ExportBoardCommand(boardId, format, jwtToken, frontendBaseUrl, token ?? "");
        var result = await _sender.Send(command);

        if (result.IsFailure) return HandleFailure(result);

        return File(result.Value.Data, result.Value.ContentType, result.Value.FileName);
    }

    [HttpPost("workspaces/{workspaceId:guid}/import")]
    [Authorize(Policy = Permissions.BoardCreate)]
    public async Task<IActionResult> Import(Guid workspaceId, [FromBody] ImportBoardRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new ImportBoardCommand(workspaceId, userId, request.Format, request.Content);
        var result = await _sender.Send(command);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}
