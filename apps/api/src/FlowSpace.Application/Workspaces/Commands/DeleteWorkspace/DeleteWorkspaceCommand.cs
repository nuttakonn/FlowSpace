using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Common.Abstractions;

namespace FlowSpace.Application.Workspaces.Commands.DeleteWorkspace;

public record DeleteWorkspaceCommand(Guid Id) : ICommand;

public class DeleteWorkspaceCommandHandler : ICommandHandler<DeleteWorkspaceCommand>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository, IUnitOfWork unitOfWork)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteWorkspaceCommand command, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(command.Id, cancellationToken);

        if (workspace is null)
        {
            return Result.Failure(new Error("Workspace.NotFound", "Workspace not found."));
        }

        _workspaceRepository.Delete(workspace);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
