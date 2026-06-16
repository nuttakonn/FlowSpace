using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Workspaces;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Common.Abstractions;

namespace FlowSpace.Application.Workspaces.Commands.UpdateWorkspace;

public record UpdateWorkspaceCommand(Guid Id, string Name) : ICommand<WorkspaceResponse>;

public class UpdateWorkspaceCommandHandler : ICommandHandler<UpdateWorkspaceCommand, WorkspaceResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository, IUnitOfWork unitOfWork)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkspaceResponse>> Handle(UpdateWorkspaceCommand command, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(command.Id, cancellationToken);

        if (workspace is null)
        {
            return Result.Failure<WorkspaceResponse>(new Error("Workspace.NotFound", "Workspace not found."));
        }

        workspace.Update(command.Name);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkspaceResponse(workspace.Id, workspace.Name, workspace.OwnerId, workspace.CreatedAt);
    }
}
