using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Workspaces;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Workspaces.Queries.GetWorkspace;

public record GetWorkspaceQuery(Guid Id) : IQuery<WorkspaceResponse>;

public class GetWorkspaceQueryHandler : IQueryHandler<GetWorkspaceQuery, WorkspaceResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public GetWorkspaceQueryHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<Result<WorkspaceResponse>> Handle(GetWorkspaceQuery query, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(query.Id, cancellationToken);

        if (workspace is null)
        {
            return Result.Failure<WorkspaceResponse>(new Error("Workspace.NotFound", "Workspace not found."));
        }

        return new WorkspaceResponse(workspace.Id, workspace.Name, workspace.OwnerId, workspace.CreatedAt);
    }
}
