using FlowSpace.Domain.Common;

namespace FlowSpace.Domain.Entities;

public class Edge : Entity, ISoftDeletable
{
    public Guid BoardId { get; private set; }
    public Board Board { get; private set; } = null!;
    public Guid SourceNodeId { get; private set; }
    public Node SourceNode { get; private set; } = null!;
    public Guid TargetNodeId { get; private set; }
    public Node TargetNode { get; private set; } = null!;
    public string Metadata { get; private set; } = "{}";
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    private Edge() { }

    public static Edge Create(Guid id, Guid boardId, Guid sourceNodeId, Guid targetNodeId)
    {
        return new Edge
        {
            Id = id,
            BoardId = boardId,
            SourceNodeId = sourceNodeId,
            TargetNodeId = targetNodeId
        };
    }

    public void SetMetadata(string metadata)
    {
        Metadata = metadata;
    }
}
