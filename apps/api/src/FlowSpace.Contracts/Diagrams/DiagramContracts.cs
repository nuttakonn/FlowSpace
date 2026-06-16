namespace FlowSpace.Contracts.Diagrams;

public record NodeResponse(
    Guid Id,
    Guid BoardId,
    string Type,
    double X,
    double Y,
    double? Width,
    double? Height,
    string Metadata,
    int Version);

public record DiagramResponse(
    List<NodeResponse> Nodes,
    List<EdgeResponse> Edges);

public record WhiteboardResponse(
    Dictionary<string, object> Records);

public record CreateNodeRequest(
    string Type,
    double X,
    double Y,
    double? Width,
    double? Height,
    string Metadata);

public record UpdateNodeRequest(
    double X,
    double Y,
    double? Width,
    double? Height,
    string Metadata);

public record EdgeResponse(
    Guid Id,
    Guid BoardId,
    Guid SourceNodeId,
    Guid TargetNodeId,
    string Metadata);

public record CreateEdgeRequest(
    Guid SourceNodeId,
    Guid TargetNodeId,
    string Metadata);
