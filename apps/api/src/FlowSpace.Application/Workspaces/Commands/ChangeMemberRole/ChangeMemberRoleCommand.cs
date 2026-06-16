using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Workspaces.Commands.ChangeMemberRole;

public record ChangeMemberRoleCommand(Guid WorkspaceId, Guid UserId, WorkspaceRole Role) : ICommand;

public class ChangeMemberRoleCommandHandler : ICommandHandler<ChangeMemberRoleCommand>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeMemberRoleCommandHandler(IWorkspaceRepository workspaceRepository, IUnitOfWork unitOfWork)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeMemberRoleCommand command, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(command.WorkspaceId, cancellationToken);
        if (workspace is null)
        {
            return Result.Failure(new Error("Workspace.NotFound", "Workspace not found."));
        }

        workspace.ChangeMemberRole(command.UserId, command.Role);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
