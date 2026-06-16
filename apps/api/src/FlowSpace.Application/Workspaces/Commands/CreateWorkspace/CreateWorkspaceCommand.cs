using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Workspaces;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Domain.Authorization;

namespace FlowSpace.Application.Workspaces.Commands.CreateWorkspace;

public record CreateWorkspaceCommand(string Name, Guid OwnerId) : ICommand<WorkspaceResponse>;

public class CreateWorkspaceCommandHandler : ICommandHandler<CreateWorkspaceCommand, WorkspaceResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository, IUnitOfWork unitOfWork)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkspaceResponse>> Handle(CreateWorkspaceCommand command, CancellationToken cancellationToken)
    {
        var workspace = Workspace.Create(Guid.NewGuid(), command.Name, command.OwnerId);
        
        _workspaceRepository.Add(workspace);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkspaceResponse(workspace.Id, workspace.Name, workspace.OwnerId, workspace.CreatedAt);
    }
}
