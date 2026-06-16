using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Diagrams.Commands.DeleteNode;

public record DeleteNodeCommand(Guid Id) : ICommand;

public class DeleteNodeCommandHandler : ICommandHandler<DeleteNodeCommand>
{
    private readonly INodeRepository _nodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNodeCommandHandler(INodeRepository nodeRepository, IUnitOfWork unitOfWork)
    {
        _nodeRepository = nodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteNodeCommand command, CancellationToken cancellationToken)
    {
        var node = await _nodeRepository.GetByIdAsync(command.Id, cancellationToken);
        if (node is null)
        {
            return Result.Failure(new Error("Node.NotFound", "Node not found."));
        }

        _nodeRepository.Delete(node);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
