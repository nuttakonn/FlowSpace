using FlowSpace.Application.Common.Abstractions.Authorization;
using FlowSpace.Domain.Authorization;
using FlowSpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FlowSpace.Infrastructure.Authorization;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);

    public PermissionService(ApplicationDbContext dbContext, IDistributedCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission, Guid? resourceId = null)
    {
        if (permission == Permissions.WorkspaceCreate) return true;
        if (resourceId == null) return false;

        // Try to resolve resource as Workspace
        if (await HasWorkspacePermissionAsync(userId, resourceId.Value, permission))
        {
            return true;
        }

        // Try to resolve resource as Board
        // A board is always in a workspace, so we first check board-specific role
        if (await HasBoardPermissionAsync(userId, resourceId.Value, permission))
        {
            return true;
        }

        // If it's a board, check if the workspace role grants access
        var board = await _dbContext.Boards
            .AsNoTracking()
            .Select(b => new { b.Id, b.WorkspaceId })
            .FirstOrDefaultAsync(b => b.Id == resourceId.Value);

        if (board != null)
        {
            if (await HasWorkspacePermissionAsync(userId, board.WorkspaceId, permission))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> HasWorkspacePermissionAsync(Guid userId, Guid workspaceId, string permission)
    {
        var cacheKey = $"permission:user:{userId}:workspace:{workspaceId}";
        
        var cachedRoleString = await _cache.GetStringAsync(cacheKey);
        WorkspaceRole? role = null;

        if (!string.IsNullOrEmpty(cachedRoleString) && int.TryParse(cachedRoleString, out var parsedRole))
        {
            role = (WorkspaceRole)parsedRole;
        }
        else
        {
            var member = await _dbContext.WorkspaceMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(wm => wm.UserId == userId && wm.WorkspaceId == workspaceId);

            if (member != null)
            {
                role = member.Role;
                await _cache.SetStringAsync(
                    cacheKey, 
                    ((int)role).ToString(), 
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl });
            }
        }

        if (role.HasValue)
        {
            return Permissions.GetForRole(role.Value).Contains(permission);
        }

        return false;
    }

    private async Task<bool> HasBoardPermissionAsync(Guid userId, Guid boardId, string permission)
    {
        var cacheKey = $"permission:user:{userId}:board:{boardId}";
        
        var cachedRoleString = await _cache.GetStringAsync(cacheKey);
        BoardRole? role = null;

        if (!string.IsNullOrEmpty(cachedRoleString) && int.TryParse(cachedRoleString, out var parsedRole))
        {
            role = (BoardRole)parsedRole;
        }
        else
        {
            var boardPermission = await _dbContext.BoardPermissions
                .AsNoTracking()
                .FirstOrDefaultAsync(bp => bp.UserId == userId && bp.BoardId == boardId);

            if (boardPermission != null)
            {
                role = boardPermission.Role;
                await _cache.SetStringAsync(
                    cacheKey, 
                    ((int)role).ToString(), 
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl });
            }
        }

        if (role.HasValue)
        {
            return Permissions.GetForRole(role.Value).Contains(permission);
        }

        return false;
    }

    public async Task<bool> HasPermissionAsync(string token, string permission, Guid boardId)
    {
        if (string.IsNullOrEmpty(token)) return false;

        var link = await _dbContext.BoardShareLinks
            .AsNoTracking()
            .Include(l => l.Board)
            .FirstOrDefaultAsync(l => l.Token == token && l.BoardId == boardId);

        // If link is null, or board is null (which happens if it's soft-deleted due to Global Query Filter), return false
        if (link == null || link.Board == null || !link.IsValid()) return false;

        return Permissions.GetForRole(link.Role).Contains(permission);
    }
}
