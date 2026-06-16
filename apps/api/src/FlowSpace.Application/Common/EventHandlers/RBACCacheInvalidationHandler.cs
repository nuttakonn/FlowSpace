using FlowSpace.Domain.Events;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FlowSpace.Application.Common.EventHandlers;

public class RBACCacheInvalidationHandler : 
    INotificationHandler<WorkspaceMemberRemovedEvent>,
    INotificationHandler<WorkspaceMemberRoleChangedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RBACCacheInvalidationHandler> _logger;

    public RBACCacheInvalidationHandler(IDistributedCache cache, ILogger<RBACCacheInvalidationHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task Handle(WorkspaceMemberRemovedEvent notification, CancellationToken cancellationToken)
    {
        var key = $"permission:user:{notification.UserId}:workspace:{notification.WorkspaceId}";
        await _cache.RemoveAsync(key, cancellationToken);
        _logger.LogInformation("Invalidated RBAC cache key: {Key}", key);
    }

    public async Task Handle(WorkspaceMemberRoleChangedEvent notification, CancellationToken cancellationToken)
    {
        var key = $"permission:user:{notification.UserId}:workspace:{notification.WorkspaceId}";
        await _cache.RemoveAsync(key, cancellationToken);
        _logger.LogInformation("Invalidated RBAC cache key: {Key}", key);
    }
}
