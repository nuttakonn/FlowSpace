namespace FlowSpace.Domain.Entities;

public class CanvasSnapshot
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Board Board { get; private set; } = null!;
    public string NodesData { get; private set; } = "[]"; // JSON string
    public string EdgesData { get; private set; } = "[]"; // JSON string
    public byte[] YjsState { get; private set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private CanvasSnapshot() { }

    public static CanvasSnapshot Create(Guid boardId, string nodesData, string edgesData, byte[] yjsState)
    {
        return new CanvasSnapshot
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            NodesData = nodesData,
            EdgesData = edgesData,
            YjsState = yjsState
        };
    }
}
