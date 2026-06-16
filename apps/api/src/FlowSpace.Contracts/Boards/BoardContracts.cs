namespace FlowSpace.Contracts.Boards;

public record CreateBoardRequest(string Name, string Type);

public record UpdateBoardRequest(string Name);

public record BoardResponse(
    Guid Id,
    Guid WorkspaceId,
    string Name,
    string Type,
    DateTime CreatedAt);
