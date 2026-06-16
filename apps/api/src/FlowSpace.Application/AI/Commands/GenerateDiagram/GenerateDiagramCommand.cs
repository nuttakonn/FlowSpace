using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.AI;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.AI;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Common.Abstractions.Authorization;
using FluentValidation;
using System.Text.Json;

namespace FlowSpace.Application.AI.Commands.GenerateDiagram;

public record GenerateDiagramCommand(Guid BoardId, Guid UserId, string Prompt, AiDiagramType Type, Guid? TemplateId = null) : ICommand;

public class GenerateDiagramCommandHandler : ICommandHandler<GenerateDiagramCommand>
{
    private readonly IAiService _aiService;
    private readonly IBoardRepository _boardRepository;
    private readonly INodeRepository _nodeRepository;
    private readonly IEdgeRepository _edgeRepository;
    private readonly IAiGenerationHistoryRepository _historyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AiDiagramResponse> _responseValidator;
    private readonly IPermissionService _permissionService;

    public GenerateDiagramCommandHandler(
        IAiService aiService,
        IBoardRepository boardRepository,
        INodeRepository nodeRepository,
        IEdgeRepository edgeRepository,
        IAiGenerationHistoryRepository historyRepository,
        IUnitOfWork unitOfWork,
        IValidator<AiDiagramResponse> responseValidator,
        IPermissionService permissionService)
    {
        _aiService = aiService;
        _boardRepository = boardRepository;
        _nodeRepository = nodeRepository;
        _edgeRepository = edgeRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
        _responseValidator = responseValidator;
        _permissionService = permissionService;
    }

    public async Task<Result> Handle(GenerateDiagramCommand command, CancellationToken cancellationToken)
    {
        // 1. Check RBAC
        if (!await _permissionService.HasPermissionAsync(command.UserId, "Node.Create", command.BoardId))
        {
            return Result.Failure(new Error("Auth.Forbidden", "You do not have permission to generate diagrams on this board."));
        }

        var board = await _boardRepository.GetByIdAsync(command.BoardId, cancellationToken);
        if (board is null) return Result.Failure(new Error("Board.NotFound", "Board not found."));

        // 2. Call AI Service with Template Context
        var aiContext = new AiDiagramContext(TemplateId: command.TemplateId);
        
        var aiRequest = new AiDiagramRequest(command.Prompt, command.Type, aiContext);
        var aiResult = await _aiService.GenerateDiagramAsync(aiRequest, cancellationToken);
        
        if (aiResult.IsFailure) return aiResult;

        // 3. Validate AI Output
        var validationResult = await _responseValidator.ValidateAsync(aiResult.Value, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("AI.InvalidResponse", "AI generated an invalid diagram structure."));
        }

        // 4. Map to Canvas Model and Persist
        var aiResponse = aiResult.Value;
        var nodeMap = new Dictionary<string, Guid>();

        double currentX = 100;
        double currentY = 100;
        int index = 0;

        foreach (var aiNode in aiResponse.Nodes)
        {
            var nodeId = Guid.NewGuid();
            nodeMap[aiNode.Id] = nodeId;

            double x = aiNode.Position?.X ?? (aiResponse.LayoutHint == "horizontal" ? currentX + (index * 250) : currentX);
            double y = aiNode.Position?.Y ?? (aiResponse.LayoutHint == "vertical" ? currentY + (index * 150) : currentY);

            var node = Node.Create(nodeId, command.BoardId, aiNode.Type, x, y);
            node.SetMetadata(aiNode.Metadata);
            _nodeRepository.Add(node);
            index++;
        }

        foreach (var aiEdge in aiResponse.Edges)
        {
            if (nodeMap.TryGetValue(aiEdge.Source, out var sourceId) && nodeMap.TryGetValue(aiEdge.Target, out var targetId))
            {
                var edge = Edge.Create(Guid.NewGuid(), command.BoardId, sourceId, targetId);
                _edgeRepository.Add(edge);
            }
        }

        // 5. Save and Publish Domain Event
        board.MarkAsAIGenerated(aiResponse.Nodes.Count);
        
        // 6. Save to History
        var history = AiGenerationRequest.Create(
            command.BoardId, 
            command.UserId, 
            command.Prompt, 
            command.Type.ToString(), 
            JsonSerializer.Serialize(aiResponse));
        
        _historyRepository.Add(history);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
