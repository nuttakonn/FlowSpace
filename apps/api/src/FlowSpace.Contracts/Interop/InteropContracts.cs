namespace FlowSpace.Contracts.Interop;

public record ExportBoardRequest(string Format); // flowspace, drawio, svg, png, pdf

public record ImportBoardRequest(string Format, string Content);

public record ExportResponse(string FileName, string ContentType, byte[] Data);

public record FlowSpaceFile(
    string Format,
    string Version,
    FlowSpaceMetadata Metadata,
    FlowSpaceCanvas Canvas);

public record FlowSpaceMetadata(
    Guid Id,
    string Name,
    string Type,
    DateTime ExportedAt,
    string Generator);

public record FlowSpaceCanvas(
    FlowSpaceViewport Viewport,
    List<FlowSpaceNode> Nodes,
    List<FlowSpaceEdge> Edges);

public record FlowSpaceViewport(double X, double Y, double Zoom);

public record FlowSpaceNode(
    string Id,
    string Type,
    FlowSpacePosition Position,
    FlowSpaceSize? Size,
    string Metadata);

public record FlowSpaceEdge(
    string Id,
    string Source,
    string Target,
    string Metadata);

public record FlowSpacePosition(double X, double Y);

public record FlowSpaceSize(double Width, double Height);
