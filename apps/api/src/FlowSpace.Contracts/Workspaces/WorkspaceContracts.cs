namespace FlowSpace.Contracts.Workspaces;

public record CreateWorkspaceRequest(string Name);

public record UpdateWorkspaceRequest(string Name);

public record WorkspaceResponse(
    Guid Id,
    string Name,
    Guid OwnerId,
    DateTime CreatedAt);
