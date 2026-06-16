using System.Security.Claims;
using FlowSpace.Application.AI.Commands.GenerateDiagram;
using FlowSpace.Application.AI.Queries.PreviewDiagram;
using FlowSpace.Application.AI.Queries.GetAiHistory;
using FlowSpace.Contracts.AI;
using FlowSpace.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSpace.Api.Controllers;

[Route("api/v1/boards/{boardId:guid}/ai")]
[Authorize]
public class AiController : ApiController
{
    private readonly ISender _sender;

    public AiController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("generate")]
    [Authorize(Policy = Permissions.NodeCreate)]
    public async Task<IActionResult> Generate(Guid boardId, [FromBody] GenerateAiDiagramRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new GenerateDiagramCommand(boardId, userId, request.Prompt, request.Type, request.TemplateId);
        var result = await _sender.Send(command);

        return result.IsSuccess ? Accepted() : HandleFailure(result);
    }

    [HttpPost("preview")]
    [Authorize(Policy = Permissions.NodeCreate)]
    public async Task<IActionResult> Preview(Guid boardId, [FromBody] PreviewAiDiagramRequest request)
    {
        var query = new PreviewDiagramQuery(
            request.Prompt, 
            request.Type, 
            request.TemplateId, 
            request.RefinementCommand, 
            request.ExistingNodes, 
            request.ExistingEdges);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet("history")]
    [Authorize(Policy = Permissions.NodeRead)]
    public async Task<IActionResult> GetHistory(Guid boardId)
    {
        var query = new GetAiHistoryQuery(boardId);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    public record GenerateAiDiagramRequest(string Prompt, AiDiagramType Type, Guid? TemplateId = null);

    public record PreviewAiDiagramRequest(
        string Prompt, 
        AiDiagramType Type, 
        Guid? TemplateId = null,
        AiRefinementCommand? RefinementCommand = null,
        List<AiNodeResponse>? ExistingNodes = null,
        List<AiEdgeResponse>? ExistingEdges = null);
}
