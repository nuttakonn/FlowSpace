using FlowSpace.Domain.Common;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Events;

namespace FlowSpace.Domain.Entities;

public enum BoardVisibility
{
    Private = 1,
    Workspace = 2,
    Public = 3
}

public class Board : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    public Guid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Type { get; private set; } = null!; // Whiteboard, Flowchart, etc.
    public BoardVisibility Visibility { get; private set; } = BoardVisibility.Private;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private readonly List<Node> _nodes = new();
    public IReadOnlyCollection<Node> Nodes => _nodes.AsReadOnly();

    private readonly List<Edge> _edges = new();
    public IReadOnlyCollection<Edge> Edges => _edges.AsReadOnly();

    private readonly List<BoardPermission> _permissions = new();
    public IReadOnlyCollection<BoardPermission> Permissions => _permissions.AsReadOnly();

    private readonly List<BoardShareLink> _shareLinks = new();
    public IReadOnlyCollection<BoardShareLink> ShareLinks => _shareLinks.AsReadOnly();

    private Board() { }

    public static Board Create(Guid id, Guid workspaceId, string name, string type, Guid creatorId)
    {
        var board = new Board
        {
            Id = id,
            WorkspaceId = workspaceId,
            Name = name,
            Type = type,
            Visibility = BoardVisibility.Private
        };

        board.AddPermission(creatorId, BoardRole.Owner);
        board.AddDomainEvent(new BoardCreatedEvent(board.Id, workspaceId));
        return board;
    }

    public void UpdateVisibility(BoardVisibility visibility)
    {
        Visibility = visibility;
    }

    public BoardShareLink CreateShareLink(BoardRole role, DateTime? expiresAt = null)
    {
        var link = BoardShareLink.Create(Id, role, expiresAt);
        _shareLinks.Add(link);
        return link;
    }

    public void Update(string name)
    {
        Name = name;
    }

    public void AddPermission(Guid userId, BoardRole role)
    {
        if (!_permissions.Any(p => p.UserId == userId))
        {
            _permissions.Add(BoardPermission.Create(Id, userId, role));
        }
    }

    public void MarkAsAIGenerated(int nodeCount)
    {
        AddDomainEvent(new DiagramGeneratedEvent(Id, nodeCount));
    }
}
