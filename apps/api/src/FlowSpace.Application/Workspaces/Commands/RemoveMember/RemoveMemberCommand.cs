using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Workspaces.Commands.RemoveMember;

public record RemoveMemberCommand(Guid WorkspaceId, Guid UserId) : ICommand;

public class RemoveMemberCommandHandler : ICommandHandler<RemoveMemberCommand>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveMemberCommandHandler(IWorkspaceRepository workspaceRepository, IUnitOfWork unitOfWork)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveMemberCommand command, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(command.WorkspaceId, cancellationToken);
        if (workspace is null)
        {
            return Result.Failure(new Error("Workspace.NotFound", "Workspace not found."));
        }

        workspace.RemoveMember(command.UserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
