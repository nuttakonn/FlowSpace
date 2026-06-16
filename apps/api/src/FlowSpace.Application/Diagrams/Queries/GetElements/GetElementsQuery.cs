using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Diagrams;
using FlowSpace.Domain.Repositories;
using FlowSpace.Domain.Entities;

namespace FlowSpace.Application.Diagrams.Queries.GetElements;

public record GetElementsQuery(Guid BoardId, double? X, double? Y, double? Width, double? Height) : IQuery<DiagramResponse>;

public record DiagramResponse(List<NodeResponse> Nodes, List<EdgeResponse> Edges);

public class GetElementsQueryHandler : IQueryHandler<GetElementsQuery, DiagramResponse>
{
    private readonly INodeRepository _nodeRepository;
    private readonly IEdgeRepository _edgeRepository;

    public GetElementsQueryHandler(INodeRepository nodeRepository, IEdgeRepository edgeRepository)
    {
        _nodeRepository = nodeRepository;
        _edgeRepository = edgeRepository;
    }

    public async Task<Result<DiagramResponse>> Handle(GetElementsQuery query, CancellationToken cancellationToken)
    {
        List<Node> nodes;
        List<Edge> edges;

        if (query.X.HasValue && query.Y.HasValue && query.Width.HasValue && query.Height.HasValue)
        {
            double vLeft = query.X.Value;
            double vRight = query.X.Value + query.Width.Value;
            double vTop = query.Y.Value;
            double vBottom = query.Y.Value + query.Height.Value;

            nodes = await _nodeRepository.GetVisibleNodesAsync(query.BoardId, vLeft, vRight, vTop, vBottom, cancellationToken);
            edges = await _edgeRepository.GetVisibleEdgesAsync(query.BoardId, vLeft, vRight, vTop, vBottom, cancellationToken);
        }
        else
        {
            nodes = await _nodeRepository.GetByBoardIdAsync(query.BoardId, cancellationToken);
            edges = await _edgeRepository.GetByBoardIdAsync(query.BoardId, cancellationToken);
        }

        var nodeResponses = nodes.Select(n => new NodeResponse(
            n.Id,
            n.BoardId,
            n.Type,
            n.X,
            n.Y,
            n.Width,
            n.Height,
            n.Metadata,
            n.Version)).ToList();

        var edgeResponses = edges.Select(e => new EdgeResponse(
            e.Id,
            e.BoardId,
            e.SourceNodeId,
            e.TargetNodeId,
            e.Metadata)).ToList();

        return new DiagramResponse(nodeResponses, edgeResponses);
    }
}
