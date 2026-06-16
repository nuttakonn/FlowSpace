using FlowSpace.Domain.Common;

namespace FlowSpace.Domain.Entities;

public class Node : Entity, ISoftDeletable
{
    public Guid BoardId { get; private set; }
    public Board Board { get; private set; } = null!;
    public string Type { get; private set; } = null!;
    public double X { get; private set; }
    public double Y { get; private set; }
    public double? Width { get; private set; }
    public double? Height { get; private set; }
    public string Metadata { get; private set; } = "{}"; // JSON string for EF
    public int Version { get; private set; } = 1;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    private Node() { }

    public static Node Create(Guid id, Guid boardId, string type, double x, double y)
    {
        return new Node
        {
            Id = id,
            BoardId = boardId,
            Type = type,
            X = x,
            Y = y
        };
    }

    public void Update(double x, double y, double? width, double? height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Version++;
    }

    public void SetMetadata(string metadata)
    {
        Metadata = metadata;
        Version++;
    }
}
