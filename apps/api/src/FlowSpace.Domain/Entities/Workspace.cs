using FlowSpace.Domain.Common;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Events;

namespace FlowSpace.Domain.Entities;

public class Workspace : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    public string Name { get; private set; } = null!;
    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private readonly List<Board> _boards = new();
    public IReadOnlyCollection<Board> Boards => _boards.AsReadOnly();

    private readonly List<WorkspaceMember> _members = new();
    public IReadOnlyCollection<WorkspaceMember> Members => _members.AsReadOnly();

    private Workspace() { }

    public static Workspace Create(Guid id, string name, Guid ownerId)
    {
        var workspace = new Workspace
        {
            Id = id,
            Name = name,
            OwnerId = ownerId
        };

        workspace.AddMember(ownerId, WorkspaceRole.Owner);
        workspace.AddDomainEvent(new WorkspaceCreatedEvent(workspace.Id, ownerId));
        return workspace;
    }

    public void Update(string name)
    {
        Name = name;
    }

    public void AddMember(Guid userId, WorkspaceRole role)
    {
        if (!_members.Any(m => m.UserId == userId))
        {
            _members.Add(WorkspaceMember.Create(Id, userId, role));
        }
    }

    public void InviteMember(Guid userId, string email, WorkspaceRole role)
    {
        if (!_members.Any(m => m.UserId == userId))
        {
            _members.Add(WorkspaceMember.Create(Id, userId, role));
            AddDomainEvent(new MemberInvitedEvent(Id, userId, email));
        }
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            if (member.Role == WorkspaceRole.Owner && _members.Count(m => m.Role == WorkspaceRole.Owner) <= 1)
            {
                // Business Rule: Cannot remove the last owner
                return; 
            }
            _members.Remove(member);
            AddDomainEvent(new WorkspaceMemberRemovedEvent(Id, userId));
        }
    }

    public void ChangeMemberRole(Guid userId, WorkspaceRole role)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            member.UpdateRole(role);
            AddDomainEvent(new WorkspaceMemberRoleChangedEvent(Id, userId, role));
        }
    }
}
