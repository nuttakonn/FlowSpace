using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Workspaces;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Workspaces.Queries.GetWorkspaceMembers;

public record GetWorkspaceMembersQuery(Guid WorkspaceId) : IQuery<List<WorkspaceMemberResponse>>;

public class GetWorkspaceMembersQueryHandler : IQueryHandler<GetWorkspaceMembersQuery, List<WorkspaceMemberResponse>>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public GetWorkspaceMembersQueryHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<Result<List<WorkspaceMemberResponse>>> Handle(GetWorkspaceMembersQuery query, CancellationToken cancellationToken)
    {
        var members = await _workspaceRepository.GetMembersAsync(query.WorkspaceId, cancellationToken);

        var response = members.Select(wm => new WorkspaceMemberResponse(
            wm.UserId,
            wm.User.Email,
            wm.User.DisplayName,
            (FlowSpace.Contracts.Authorization.WorkspaceRole)wm.Role,
            wm.JoinedAt)).ToList();

        return response;
    }
}
