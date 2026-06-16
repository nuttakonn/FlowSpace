using FlowSpace.Contracts.Interop;
using FlowSpace.Domain.Entities;
using System.Text.Json;

namespace FlowSpace.Application.Interop.Converters;

public static class FlowSpaceConverter
{
    public static FlowSpaceFile ToNative(Board board, List<Node> nodes, List<Edge> edges)
    {
        return new FlowSpaceFile(
            "flowspace",
            "1.0.0",
            new FlowSpaceMetadata(
                board.Id,
                board.Name,
                board.Type,
                DateTime.UtcNow,
                "FlowSpace-Backend-1.0"
            ),
            new FlowSpaceCanvas(
                new FlowSpaceViewport(0, 0, 1),
                nodes.Select(n => new FlowSpaceNode(
                    n.Id.ToString(),
                    n.Type,
                    new FlowSpacePosition(n.X, n.Y),
                    n.Width.HasValue && n.Height.HasValue ? new FlowSpaceSize(n.Width.Value, n.Height.Value) : null,
                    n.Metadata
                )).ToList(),
                edges.Select(e => new FlowSpaceEdge(
                    e.Id.ToString(),
                    e.SourceNodeId.ToString(),
                    e.TargetNodeId.ToString(),
                    e.Metadata
                )).ToList()
            )
        );
    }

    public static (List<Node> Nodes, List<Edge> Edges) FromNative(Guid boardId, FlowSpaceFile file)
    {
        var nodeMap = new Dictionary<string, Guid>();
        var nodes = file.Canvas.Nodes.Select(n => {
            var newId = Guid.NewGuid();
            nodeMap[n.Id] = newId;
            var node = Node.Create(newId, boardId, n.Type, n.Position.X, n.Position.Y);
            if (n.Size != null) node.Update(n.Position.X, n.Position.Y, n.Size.Width, n.Size.Height);
            node.SetMetadata(n.Metadata);
            return node;
        }).ToList();

        var edges = file.Canvas.Edges.Select(e => {
            var sourceId = nodeMap.ContainsKey(e.Source) ? nodeMap[e.Source] : Guid.Parse(e.Source);
            var targetId = nodeMap.ContainsKey(e.Target) ? nodeMap[e.Target] : Guid.Parse(e.Target);
            var edge = Edge.Create(Guid.NewGuid(), boardId, sourceId, targetId);
            edge.SetMetadata(e.Metadata);
            return edge;
        }).ToList();

        return (nodes, edges);
    }
}
