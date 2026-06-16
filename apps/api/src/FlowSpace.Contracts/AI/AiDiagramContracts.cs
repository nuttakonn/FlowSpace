namespace FlowSpace.Contracts.AI;

public enum AiDiagramType
{
    Flowchart = 1,
    Mindmap = 2,
    SystemArchitecture = 3,
    UserJourney = 4
}

public enum AiRefinementCommand
{
    ImproveArchitecture = 1,
    AddMissingComponents = 2,
    SimplifyDiagram = 3,
    ExplainDiagram = 4
}

public record AiDiagramRequest(
    string Prompt,
    AiDiagramType DiagramType,
    AiDiagramContext? Context = null,
    AiDiagramConstraints? Constraints = null,
    AiRefinementCommand? RefinementCommand = null);

public record AiDiagramContext(
    List<string>? ExistingNodeIds = null,
    string? Theme = "modern",
    Guid? TemplateId = null,
    List<AiNodeResponse>? ExistingNodes = null,
    List<AiEdgeResponse>? ExistingEdges = null);

public record AiDiagramConstraints(
    int? MaxNodes = 20,
    string? Language = "English");

public record AiNodeResponse(
    string Id,
    string Type,
    AiNodePosition? Position,
    AiNodeData Data,
    string Metadata); // JSON string for flexible attributes

public record AiNodePosition(double X, double Y);

public record AiNodeData(
    string Label,
    string? Description = null);

public record AiEdgeResponse(
    string Id,
    string Source,
    string Target,
    string? Label = null);

public record AiDiagramResponse(
    string SchemaVersion,
    List<AiNodeResponse> Nodes,
    List<AiEdgeResponse> Edges,
    string? LayoutHint = "vertical");

public record AiGenerationHistoryResponse(
    Guid Id,
    Guid BoardId,
    string Prompt,
    string DiagramType,
    AiDiagramResponse Result,
    DateTime CreatedAt,
    string CreatorName);
