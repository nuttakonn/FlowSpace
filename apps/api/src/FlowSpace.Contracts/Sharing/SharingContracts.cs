namespace FlowSpace.Contracts.Sharing;

public record ShareLinkResponse(
    Guid Id,
    string Token,
    int Role,
    DateTime? ExpiresAt,
    bool IsRevoked,
    DateTime CreatedAt,
    string Url);

public record BoardSharingInfoResponse(
    Guid BoardId,
    int Visibility,
    List<BoardPermissionResponse> Permissions,
    List<ShareLinkResponse> ShareLinks);

public record BoardPermissionResponse(
    Guid UserId,
    string UserEmail,
    string DisplayName,
    int Role);

public record CreateShareLinkRequest(
    int Role,
    DateTime? ExpiresAt = null);

public record UpdateBoardVisibilityRequest(
    int Visibility);

public record InviteBoardMemberRequest(
    string Email,
    int Role);
