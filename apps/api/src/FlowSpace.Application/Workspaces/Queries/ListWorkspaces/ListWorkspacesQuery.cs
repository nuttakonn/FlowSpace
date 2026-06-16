using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Workspaces;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Workspaces.Queries.ListWorkspaces;

public record ListWorkspacesQuery(Guid UserId) : IQuery<List<WorkspaceResponse>>;

public class ListWorkspacesQueryHandler : IQueryHandler<ListWorkspacesQuery, List<WorkspaceResponse>>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public ListWorkspacesQueryHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<Result<List<WorkspaceResponse>>> Handle(ListWorkspacesQuery query, CancellationToken cancellationToken)
    {
        var workspaces = await _workspaceRepository.ListByUserIdAsync(query.UserId, cancellationToken);

        var response = workspaces.Select(w => new WorkspaceResponse(w.Id, w.Name, w.OwnerId, w.CreatedAt)).ToList();

        return response;
    }
}
