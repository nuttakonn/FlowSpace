namespace FlowSpace.Contracts.Templates;

public record BoardTemplateResponse(
    Guid Id,
    string Name,
    string Description,
    string ThumbnailUrl,
    string BoardType,
    bool IsSystem,
    DateTime CreatedAt);
