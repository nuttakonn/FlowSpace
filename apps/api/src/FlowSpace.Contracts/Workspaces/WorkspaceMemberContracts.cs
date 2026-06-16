using FlowSpace.Contracts.Authorization;

namespace FlowSpace.Contracts.Workspaces;

public record InviteMemberRequest(string Email, WorkspaceRole Role);

public record ChangeRoleRequest(WorkspaceRole Role);

public record WorkspaceMemberResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    WorkspaceRole Role,
    DateTime JoinedAt);
