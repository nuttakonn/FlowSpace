using FlowSpace.Application.Versions.Commands.CreateVersion;
using FlowSpace.Application.Versions.Commands.RestoreVersion;
using FlowSpace.Application.Versions.Queries.GetVersionDetails;
using FlowSpace.Application.Versions.Queries.GetVersions;
using FlowSpace.Contracts.Versions;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowSpace.Api.Controllers;

[Route("api/v1/boards/{boardId:guid}/versions")]
[Authorize]
public class VersionsController : ApiController
{
    private readonly ISender _sender;

    public VersionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.NodeRead)]
    public async Task<IActionResult> GetVersions(Guid boardId)
    {
        var query = new GetVersionsQuery(boardId);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.BoardUpdate)]
    public async Task<IActionResult> CreateVersion(Guid boardId, CreateVersionRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateVersionCommand(
            boardId, 
            userId, 
            request.Name, 
            request.Description, 
            request.NodesData, 
            request.EdgesData, 
            request.YjsState, 
            VersionType.Manual);

        var result = await _sender.Send(command);

        return result.IsSuccess ? CreatedAtAction(nameof(GetVersionDetails), new { boardId, versionId = result.Value.Id }, result.Value) : HandleFailure(result);
    }

    [HttpGet("{versionId:guid}")]
    [Authorize(Policy = Permissions.NodeRead)]
    public async Task<IActionResult> GetVersionDetails(Guid boardId, Guid versionId)
    {
        var query = new GetVersionDetailsQuery(versionId);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPost("{versionId:guid}/restore")]
    [Authorize(Policy = Permissions.BoardUpdate)]
    public async Task<IActionResult> RestoreVersion(Guid boardId, Guid versionId)
    {
        var command = new RestoreVersionCommand(versionId);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}
