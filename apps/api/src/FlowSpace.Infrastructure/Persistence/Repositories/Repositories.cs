using Microsoft.EntityFrameworkCore;
using FlowSpace.Domain.Repositories;
using FlowSpace.Domain.Entities;

namespace FlowSpace.Infrastructure.Persistence.Repositories;

public abstract class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext DbContext;

    protected Repository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public void Add(T entity)
    {
        DbContext.Set<T>().Add(entity);
    }

    public void Update(T entity)
    {
        DbContext.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        DbContext.Set<T>().Remove(entity);
    }
}

public class UserRepository : Repository<FlowSpace.Domain.Entities.User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<FlowSpace.Domain.Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<FlowSpace.Domain.Entities.User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await DbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.TokenHash == tokenHash && rt.RevokedAt == null), cancellationToken);
    }
}

public class WorkspaceRepository : Repository<FlowSpace.Domain.Entities.Workspace>, IWorkspaceRepository
{
    public WorkspaceRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public override async Task<FlowSpace.Domain.Entities.Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<List<FlowSpace.Domain.Entities.Workspace>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Workspaces
            .Where(w => w.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FlowSpace.Domain.Entities.WorkspaceMember>> GetMembersAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await DbContext.WorkspaceMembers
            .AsNoTracking()
            .Include(wm => wm.User)
            .Where(wm => wm.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<FlowSpace.Domain.Entities.Workspace?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbContext.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Name.ToLower() == name.ToLower(), cancellationToken);
    }
}

public class BoardRepository : Repository<FlowSpace.Domain.Entities.Board>, IBoardRepository
{
    public BoardRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<List<FlowSpace.Domain.Entities.Board>> ListByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Boards
            .Where(b => b.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);
    }
}

public class NodeRepository : Repository<FlowSpace.Domain.Entities.Node>, INodeRepository
{
    public NodeRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<List<FlowSpace.Domain.Entities.Node>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Nodes
            .AsNoTracking()
            .Where(n => n.BoardId == boardId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FlowSpace.Domain.Entities.Node>> GetVisibleNodesAsync(Guid boardId, double vLeft, double vRight, double vTop, double vBottom, CancellationToken cancellationToken = default)
    {
        return await DbContext.Nodes
            .AsNoTracking()
            .Where(n => n.BoardId == boardId &&
                        n.X <= vRight &&
                        (n.X + (n.Width ?? 200)) >= vLeft &&
                        n.Y <= vBottom &&
                        (n.Y + (n.Height ?? 100)) >= vTop)
            .ToListAsync(cancellationToken);
    }
}

public class EdgeRepository : Repository<FlowSpace.Domain.Entities.Edge>, IEdgeRepository
{
    public EdgeRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<List<FlowSpace.Domain.Entities.Edge>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Edges
            .AsNoTracking()
            .Where(e => e.BoardId == boardId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FlowSpace.Domain.Entities.Edge>> GetVisibleEdgesAsync(Guid boardId, double vLeft, double vRight, double vTop, double vBottom, CancellationToken cancellationToken = default)
    {
        return await DbContext.Edges
            .AsNoTracking()
            .Where(e => e.BoardId == boardId &&
                (DbContext.Nodes.Any(n => n.Id == e.SourceNodeId && n.BoardId == boardId &&
                    n.X <= vRight && (n.X + (n.Width ?? 200)) >= vLeft && n.Y <= vBottom && (n.Y + (n.Height ?? 100)) >= vTop)
                ||
                DbContext.Nodes.Any(n => n.Id == e.TargetNodeId && n.BoardId == boardId &&
                    n.X <= vRight && (n.X + (n.Width ?? 200)) >= vLeft && n.Y <= vBottom && (n.Y + (n.Height ?? 100)) >= vTop)))
            .ToListAsync(cancellationToken);
    }
}

public class BoardVersionRepository : Repository<BoardVersion>, IBoardVersionRepository
{
    public BoardVersionRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<List<BoardVersion>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        return await DbContext.BoardVersions
            .AsNoTracking()
            .Include(v => v.Creator)
            .Where(v => v.BoardId == boardId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<BoardVersion?> GetWithSnapshotAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.BoardVersions
            .Include(v => v.Snapshot)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }
}

public class CanvasSnapshotRepository : Repository<CanvasSnapshot>, ICanvasSnapshotRepository
{
    public CanvasSnapshotRepository(ApplicationDbContext dbContext) : base(dbContext) { }
}

public class AiGenerationHistoryRepository : Repository<AiGenerationRequest>, IAiGenerationHistoryRepository
{
    public AiGenerationHistoryRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<List<AiGenerationRequest>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        return await DbContext.AiGenerationRequests
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.BoardId == boardId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

public class BoardTemplateRepository : Repository<BoardTemplate>, IBoardTemplateRepository
{
    public BoardTemplateRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<List<BoardTemplate>> ListAsync(string? boardType = null, bool? isSystem = null, CancellationToken cancellationToken = default)
    {
        var query = DbContext.BoardTemplates.AsNoTracking();

        if (!string.IsNullOrEmpty(boardType)) query = query.Where(t => t.BoardType == boardType);
        if (isSystem.HasValue) query = query.Where(t => t.IsSystem == isSystem.Value);

        return await query.ToListAsync(cancellationToken);
    }
}

public class BoardShareLinkRepository : Repository<BoardShareLink>, IBoardShareLinkRepository
{
    public BoardShareLinkRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<BoardShareLink?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await DbContext.BoardShareLinks
            .Include(sl => sl.Board)
            .FirstOrDefaultAsync(sl => sl.Token == token, cancellationToken);
    }

    public async Task<List<BoardShareLink>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        return await DbContext.BoardShareLinks
            .AsNoTracking()
            .Where(sl => sl.BoardId == boardId)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
