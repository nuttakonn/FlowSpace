using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Workspaces.Commands.JoinWorkspace;

public record JoinWorkspaceCommand(Guid WorkspaceId, Guid UserId) : ICommand;

public class JoinWorkspaceCommandHandler : ICommandHandler<JoinWorkspaceCommand>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public JoinWorkspaceCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(JoinWorkspaceCommand command, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(command.WorkspaceId, cancellationToken);
        if (workspace is null)
        {
            return Result.Failure(new Error("Workspace.NotFound", "Workspace not found."));
        }

        // If user is already a member, return success (idempotent)
        if (workspace.Members.Any(m => m.UserId == command.UserId))
        {
            return Result.Success();
        }

        workspace.AddMember(command.UserId, WorkspaceRole.Editor);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
