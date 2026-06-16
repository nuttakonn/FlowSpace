using FlowSpace.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowSpace.Application.Common.EventHandlers;

public class WorkspaceCreatedEventHandler : INotificationHandler<WorkspaceCreatedEvent>
{
    private readonly ILogger<WorkspaceCreatedEventHandler> _logger;

    public WorkspaceCreatedEventHandler(ILogger<WorkspaceCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(WorkspaceCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Workspace {WorkspaceId} was created by Owner {OwnerId}.", notification.WorkspaceId, notification.OwnerId);
        
        // Future: Send welcome email to workspace owner or provision default templates.
        
        return Task.CompletedTask;
    }
}
