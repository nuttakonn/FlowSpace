using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Diagrams;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Diagrams.Commands.CreateEdge;

public record CreateEdgeCommand(
    Guid BoardId,
    Guid SourceNodeId,
    Guid TargetNodeId,
    string Metadata) : ICommand<EdgeResponse>;

public class CreateEdgeCommandHandler : ICommandHandler<CreateEdgeCommand, EdgeResponse>
{
    private readonly IEdgeRepository _edgeRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly INodeRepository _nodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEdgeCommandHandler(
        IEdgeRepository edgeRepository,
        IBoardRepository boardRepository,
        INodeRepository nodeRepository,
        IUnitOfWork unitOfWork)
    {
        _edgeRepository = edgeRepository;
        _boardRepository = boardRepository;
        _nodeRepository = nodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EdgeResponse>> Handle(CreateEdgeCommand command, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, cancellationToken);
        if (board is null)
        {
            return Result.Failure<EdgeResponse>(new Error("Board.NotFound", "Board not found."));
        }

        var sourceNode = await _nodeRepository.GetByIdAsync(command.SourceNodeId, cancellationToken);
        var targetNode = await _nodeRepository.GetByIdAsync(command.TargetNodeId, cancellationToken);

        if (sourceNode is null || targetNode is null)
        {
            return Result.Failure<EdgeResponse>(new Error("Node.NotFound", "One or both nodes not found."));
        }

        var edge = Edge.Create(Guid.NewGuid(), command.BoardId, command.SourceNodeId, command.TargetNodeId);
        if (!string.IsNullOrEmpty(command.Metadata))
        {
            edge.SetMetadata(command.Metadata);
        }

        _edgeRepository.Add(edge);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EdgeResponse(
            edge.Id,
            edge.BoardId,
            edge.SourceNodeId,
            edge.TargetNodeId,
            edge.Metadata);
    }
}
