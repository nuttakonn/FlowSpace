using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Workspaces;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Workspaces.Queries.FindWorkspace;

public record FindWorkspaceQuery(string Query) : IQuery<WorkspaceResponse>;

public class FindWorkspaceQueryHandler : IQueryHandler<FindWorkspaceQuery, WorkspaceResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public FindWorkspaceQueryHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<Result<WorkspaceResponse>> Handle(FindWorkspaceQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Query))
        {
            return Result.Failure<WorkspaceResponse>(new Error("Workspace.InvalidQuery", "Query cannot be empty."));
        }

        var normalizedQuery = query.Query.Trim();
        
        // 1. Check if it's a valid Guid ID
        if (Guid.TryParse(normalizedQuery, out var id))
        {
            var workspace = await _workspaceRepository.GetByIdAsync(id, cancellationToken);
            if (workspace is not null)
            {
                return new WorkspaceResponse(workspace.Id, workspace.Name, workspace.OwnerId, workspace.CreatedAt);
            }
        }

        // 2. Check if a workspace exists with a deterministic Guid derived from the name
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(normalizedQuery);
        byte[] hashBytes = System.Security.Cryptography.MD5.HashData(nameBytes);
        var deterministicId = new Guid(hashBytes);

        var workspaceByDeterministicId = await _workspaceRepository.GetByIdAsync(deterministicId, cancellationToken);
        if (workspaceByDeterministicId is not null)
        {
            return new WorkspaceResponse(workspaceByDeterministicId.Id, workspaceByDeterministicId.Name, workspaceByDeterministicId.OwnerId, workspaceByDeterministicId.CreatedAt);
        }

        // 3. Finally, try searching by Name directly (for existing seeded workspaces with non-deterministic GUIDs)
        var workspaceByName = await _workspaceRepository.GetByNameAsync(normalizedQuery, cancellationToken);
        if (workspaceByName is not null)
        {
            return new WorkspaceResponse(workspaceByName.Id, workspaceByName.Name, workspaceByName.OwnerId, workspaceByName.CreatedAt);
        }

        return Result.Failure<WorkspaceResponse>(new Error("Workspace.NotFound", "Workspace not found."));
    }
}
