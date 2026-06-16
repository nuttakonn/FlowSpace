using FlowSpace.Domain.Authorization;

namespace FlowSpace.Domain.Entities;

public class WorkspaceMember
{
    public Guid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public WorkspaceRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;

    private WorkspaceMember() { }

    public static WorkspaceMember Create(Guid workspaceId, Guid userId, WorkspaceRole role)
    {
        return new WorkspaceMember
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            Role = role
        };
    }

    public void UpdateRole(WorkspaceRole role)
    {
        Role = role;
    }
}
