using FlowSpace.Application.Common.Abstractions.AI;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.AI;
using FlowSpace.Contracts.Diagrams;
using FluentValidation;

namespace FlowSpace.Application.AI.Queries.PreviewDiagram;

public record PreviewDiagramQuery(
    string Prompt, 
    AiDiagramType Type, 
    Guid? TemplateId = null,
    AiRefinementCommand? RefinementCommand = null,
    List<AiNodeResponse>? ExistingNodes = null,
    List<AiEdgeResponse>? ExistingEdges = null) : IQuery<DiagramResponse>;

public class PreviewDiagramQueryHandler : IQueryHandler<PreviewDiagramQuery, DiagramResponse>
{
    private readonly IAiService _aiService;
    private readonly IValidator<AiDiagramResponse> _responseValidator;

    public PreviewDiagramQueryHandler(IAiService aiService, IValidator<AiDiagramResponse> responseValidator)
    {
        _aiService = aiService;
        _responseValidator = responseValidator;
    }

    public async Task<Result<DiagramResponse>> Handle(PreviewDiagramQuery query, CancellationToken cancellationToken)
    {
        var aiContext = new AiDiagramContext(
            TemplateId: query.TemplateId, 
            ExistingNodes: query.ExistingNodes, 
            ExistingEdges: query.ExistingEdges);

        var aiRequest = new AiDiagramRequest(query.Prompt, query.Type, aiContext, RefinementCommand: query.RefinementCommand);
        var aiResult = await _aiService.GenerateDiagramAsync(aiRequest, cancellationToken);

        if (aiResult.IsFailure) return Result.Failure<DiagramResponse>(aiResult.Error);

        var validationResult = await _responseValidator.ValidateAsync(aiResult.Value, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<DiagramResponse>(new Error("AI.InvalidResponse", "AI generated an invalid diagram structure."));
        }

        var aiResponse = aiResult.Value;
        
        // Apply layout logic (simplified for preview)
        double currentX = 100;
        double currentY = 100;
        int index = 0;

        var nodeResponses = aiResponse.Nodes.Select(n => {
            double x = n.Position?.X ?? (aiResponse.LayoutHint == "horizontal" ? currentX + (index * 250) : currentX);
            double y = n.Position?.Y ?? (aiResponse.LayoutHint == "vertical" ? currentY + (index * 150) : currentY);
            index++;
            return new NodeResponse(Guid.NewGuid(), Guid.Empty, n.Type, x, y, null, null, n.Metadata, 1);
        }).ToList();

        // Create a temporary ID mapping for edges
        var nodeMap = aiResponse.Nodes.Zip(nodeResponses, (ai, res) => new { aiId = ai.Id, resId = res.Id })
            .ToDictionary(x => x.aiId, x => x.resId);

        var edgeResponses = aiResponse.Edges
            .Where(e => nodeMap.ContainsKey(e.Source) && nodeMap.ContainsKey(e.Target))
            .Select(e => new EdgeResponse(Guid.NewGuid(), Guid.Empty, nodeMap[e.Source], nodeMap[e.Target], "{}"))
            .ToList();

        return new DiagramResponse(nodeResponses, edgeResponses);
    }
}
