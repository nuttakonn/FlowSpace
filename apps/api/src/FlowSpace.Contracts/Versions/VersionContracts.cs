namespace FlowSpace.Contracts.Versions;

public record CreateVersionRequest(string? Name, string? Description, string NodesData, string EdgesData, byte[] YjsState);

public record BoardVersionResponse(
    Guid Id,
    Guid BoardId,
    string? Name,
    string? Description,
    DateTime CreatedAt,
    Guid CreatedBy,
    string CreatorName,
    int Type);

public record VersionDetailResponse(
    Guid Id,
    string NodesData,
    string EdgesData,
    byte[] YjsState);
