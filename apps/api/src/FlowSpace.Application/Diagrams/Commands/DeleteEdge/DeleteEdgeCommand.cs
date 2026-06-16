using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Diagrams.Commands.DeleteEdge;

public record DeleteEdgeCommand(Guid Id) : ICommand;

public class DeleteEdgeCommandHandler : ICommandHandler<DeleteEdgeCommand>
{
    private readonly IEdgeRepository _edgeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEdgeCommandHandler(IEdgeRepository edgeRepository, IUnitOfWork unitOfWork)
    {
        _edgeRepository = edgeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteEdgeCommand command, CancellationToken cancellationToken)
    {
        var edge = await _edgeRepository.GetByIdAsync(command.Id, cancellationToken);
        if (edge is null)
        {
            return Result.Failure(new Error("Edge.NotFound", "Edge not found."));
        }

        _edgeRepository.Delete(edge);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
