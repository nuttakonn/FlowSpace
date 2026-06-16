using FlowSpace.Domain.Entities;

namespace FlowSpace.Domain.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IUserRepository : IRepository<User> 
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
public interface IWorkspaceRepository : IRepository<Workspace> 
{
    Task<List<Workspace>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<WorkspaceMember>> GetMembersAsync(Guid workspaceId, CancellationToken cancellationToken = default);
}
public interface IBoardRepository : IRepository<Board> 
{
    Task<List<Board>> ListByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
}

public interface INodeRepository : IRepository<Node> 
{
    Task<List<Node>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<List<Node>> GetVisibleNodesAsync(Guid boardId, double vLeft, double vRight, double vTop, double vBottom, CancellationToken cancellationToken = default);
}

public interface IEdgeRepository : IRepository<Edge> 
{
    Task<List<Edge>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<List<Edge>> GetVisibleEdgesAsync(Guid boardId, double vLeft, double vRight, double vTop, double vBottom, CancellationToken cancellationToken = default);
}

public interface IBoardVersionRepository : IRepository<BoardVersion>
{
    Task<List<BoardVersion>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<BoardVersion?> GetWithSnapshotAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ICanvasSnapshotRepository : IRepository<CanvasSnapshot> { }

public interface IAiGenerationHistoryRepository : IRepository<AiGenerationRequest>
{
    Task<List<AiGenerationRequest>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default);
}

public interface IBoardTemplateRepository : IRepository<BoardTemplate>
{
    Task<List<BoardTemplate>> ListAsync(string? boardType = null, bool? isSystem = null, CancellationToken cancellationToken = default);
}

public interface IBoardShareLinkRepository : IRepository<BoardShareLink>
{
    Task<BoardShareLink?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<BoardShareLink>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default);
}
