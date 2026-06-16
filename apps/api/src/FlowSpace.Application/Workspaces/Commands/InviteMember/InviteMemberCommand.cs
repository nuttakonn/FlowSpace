using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Workspaces.Commands.InviteMember;

public record InviteMemberCommand(Guid WorkspaceId, string Email, WorkspaceRole Role) : ICommand;

public class InviteMemberCommandHandler : ICommandHandler<InviteMemberCommand>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InviteMemberCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(InviteMemberCommand command, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(command.WorkspaceId, cancellationToken);
        if (workspace is null)
        {
            return Result.Failure(new Error("Workspace.NotFound", "Workspace not found."));
        }

        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null)
        {
            return Result.Failure(new Error("User.NotFound", $"User with email {command.Email} not found."));
        }

        if (workspace.Members.Any(m => m.UserId == user.Id))
        {
            return Result.Failure(new Error("Workspace.AlreadyMember", "User is already a member of this workspace."));
        }

        workspace.InviteMember(user.Id, command.Email, command.Role);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
