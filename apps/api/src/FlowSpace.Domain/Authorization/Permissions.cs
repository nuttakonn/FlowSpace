namespace FlowSpace.Domain.Authorization;

public enum WorkspaceRole
{
    Owner = 1,
    Editor = 2,
    Viewer = 3
}

public enum BoardRole
{
    Owner = 1,
    Editor = 2,
    Commenter = 3,
    Viewer = 4
}

public static class Permissions
{
    public const string WorkspaceCreate = "Workspace.Create";
    public const string WorkspaceUpdate = "Workspace.Update";
    public const string WorkspaceDelete = "Workspace.Delete";

    public const string BoardCreate = "Board.Create";
    public const string BoardUpdate = "Board.Update";
    public const string BoardDelete = "Board.Delete";
    public const string BoardShare = "Board.Share";

    public const string NodeRead = "Node.Read";
    public const string NodeCreate = "Node.Create";
    public const string NodeUpdate = "Node.Update";
    public const string NodeDelete = "Node.Delete";

    public static IReadOnlyCollection<string> GetForRole(WorkspaceRole role) => role switch
    {
        WorkspaceRole.Owner => new[] { WorkspaceUpdate, WorkspaceDelete, BoardCreate, BoardUpdate, BoardDelete, NodeRead, NodeCreate, NodeUpdate, NodeDelete },
        WorkspaceRole.Editor => new[] { BoardCreate, BoardUpdate, NodeRead, NodeCreate, NodeUpdate },
        WorkspaceRole.Viewer => new[] { NodeRead },
        _ => Array.Empty<string>()
    };

    public static IReadOnlyCollection<string> GetForRole(BoardRole role) => role switch
    {
        BoardRole.Owner => new[] { BoardUpdate, BoardDelete, BoardShare, NodeRead, NodeCreate, NodeUpdate, NodeDelete },
        BoardRole.Editor => new[] { NodeRead, NodeCreate, NodeUpdate },
        BoardRole.Commenter => new[] { NodeRead },
        BoardRole.Viewer => new[] { NodeRead },
        _ => Array.Empty<string>()
    };
}
