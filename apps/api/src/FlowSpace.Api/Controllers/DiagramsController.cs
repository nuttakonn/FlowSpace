using FlowSpace.Application.Diagrams.Commands.CreateEdge;
using FlowSpace.Application.Diagrams.Commands.CreateNode;
using FlowSpace.Application.Diagrams.Commands.DeleteEdge;
using FlowSpace.Application.Diagrams.Commands.DeleteNode;
using FlowSpace.Application.Diagrams.Commands.UpdateNode;
using FlowSpace.Application.Diagrams.Queries.GetElements;
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

    public DiagramsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.NodeRead)]
    public async Task<IActionResult> GetElements(Guid boardId, [FromQuery] double? x, [FromQuery] double? y, [FromQuery] double? width, [FromQuery] double? height)
    {
        var query = new GetElementsQuery(boardId, x, y, width, height);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPost("nodes")]
    [Authorize(Policy = Permissions.NodeCreate)]
    public async Task<IActionResult> CreateNode(Guid boardId, CreateNodeRequest request)
    {
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
    [Authorize(Policy = Permissions.NodeUpdate)]
    public async Task<IActionResult> UpdateNode(Guid boardId, Guid id, UpdateNodeRequest request)
    {
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
    [Authorize(Policy = Permissions.NodeDelete)]
    public async Task<IActionResult> DeleteNode(Guid boardId, Guid id)
    {
        var command = new DeleteNodeCommand(id);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }

    [HttpPost("edges")]
    [Authorize(Policy = Permissions.NodeCreate)] // Re-using NodeCreate for edges for now
    public async Task<IActionResult> CreateEdge(Guid boardId, CreateEdgeRequest request)
    {
        var command = new CreateEdgeCommand(
            boardId,
            request.SourceNodeId,
            request.TargetNodeId,
            request.Metadata);

        var result = await _sender.Send(command);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpDelete("edges/{id:guid}")]
    [Authorize(Policy = Permissions.NodeDelete)]
    public async Task<IActionResult> DeleteEdge(Guid boardId, Guid id)
    {
        var command = new DeleteEdgeCommand(id);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}
