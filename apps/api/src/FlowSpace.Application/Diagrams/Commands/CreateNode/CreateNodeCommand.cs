using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Diagrams;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Diagrams.Commands.CreateNode;

public record CreateNodeCommand(
    Guid BoardId,
    string Type,
    double X,
    double Y,
    double? Width,
    double? Height,
    string Metadata) : ICommand<NodeResponse>;

public class CreateNodeCommandHandler : ICommandHandler<CreateNodeCommand, NodeResponse>
{
    private readonly INodeRepository _nodeRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNodeCommandHandler(INodeRepository nodeRepository, IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _nodeRepository = nodeRepository;
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NodeResponse>> Handle(CreateNodeCommand command, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, cancellationToken);
        if (board is null)
        {
            return Result.Failure<NodeResponse>(new Error("Board.NotFound", "Board not found."));
        }

        var node = Node.Create(Guid.NewGuid(), command.BoardId, command.Type, command.X, command.Y);
        if (!string.IsNullOrEmpty(command.Metadata))
        {
            node.SetMetadata(command.Metadata);
        }
        
        // Update methods should handle double? width/height if we add them to the entity Create
        // For now let's just update after creation for simplicity if we didn't add them to Create
        node.Update(command.X, command.Y, command.Width, command.Height);

        _nodeRepository.Add(node);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new NodeResponse(
            node.Id,
            node.BoardId,
            node.Type,
            node.X,
            node.Y,
            node.Width,
            node.Height,
            node.Metadata,
            node.Version);
    }
}
