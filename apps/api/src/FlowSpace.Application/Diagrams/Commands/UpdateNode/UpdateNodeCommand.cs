using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Diagrams;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Diagrams.Commands.UpdateNode;

public record UpdateNodeCommand(
    Guid Id,
    double X,
    double Y,
    double? Width,
    double? Height,
    string Metadata) : ICommand<NodeResponse>;

public class UpdateNodeCommandHandler : ICommandHandler<UpdateNodeCommand, NodeResponse>
{
    private readonly INodeRepository _nodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNodeCommandHandler(INodeRepository nodeRepository, IUnitOfWork unitOfWork)
    {
        _nodeRepository = nodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NodeResponse>> Handle(UpdateNodeCommand command, CancellationToken cancellationToken)
    {
        var node = await _nodeRepository.GetByIdAsync(command.Id, cancellationToken);
        if (node is null)
        {
            return Result.Failure<NodeResponse>(new Error("Node.NotFound", "Node not found."));
        }

        node.Update(command.X, command.Y, command.Width, command.Height);
        if (command.Metadata != null)
        {
            node.SetMetadata(command.Metadata);
        }

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
