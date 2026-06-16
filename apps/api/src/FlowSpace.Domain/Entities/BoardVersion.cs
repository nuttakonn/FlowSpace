namespace FlowSpace.Domain.Entities;

public enum VersionType
{
    Manual = 1,
    Automatic = 2
}

public class BoardVersion
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Board Board { get; private set; } = null!;
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; private set; }
    public User Creator { get; private set; } = null!;
    public Guid SnapshotId { get; private set; }
    public CanvasSnapshot Snapshot { get; private set; } = null!;
    public VersionType Type { get; private set; }

    private BoardVersion() { }

    public static BoardVersion Create(
        Guid boardId, 
        Guid createdBy, 
        Guid snapshotId, 
        VersionType type, 
        string? name = null, 
        string? description = null)
    {
        return new BoardVersion
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            CreatedBy = createdBy,
            SnapshotId = snapshotId,
            Type = type,
            Name = name,
            Description = description
        };
    }
}
