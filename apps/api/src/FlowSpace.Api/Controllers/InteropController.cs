using System.Security.Claims;
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

    public InteropController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("boards/{boardId:guid}/export")]
    [Authorize(Policy = Permissions.NodeRead)]
    public async Task<IActionResult> Export(Guid boardId, [FromQuery] string format)
    {
        string authHeader = Request.Headers["Authorization"].ToString();
        string token = "";
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authHeader.Substring("Bearer ".Length).Trim();
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

        var command = new ExportBoardCommand(boardId, format, token, frontendBaseUrl);
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
