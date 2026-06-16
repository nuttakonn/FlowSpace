using FlowSpace.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowSpace.Application.Common.EventHandlers;

public class BoardCreatedEventHandler : INotificationHandler<BoardCreatedEvent>
{
    private readonly ILogger<BoardCreatedEventHandler> _logger;

    public BoardCreatedEventHandler(ILogger<BoardCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(BoardCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: Board {BoardId} was created in Workspace {WorkspaceId}.", notification.BoardId, notification.WorkspaceId);
        
        // Future: Initialize board in real-time engine or search index.
        
        return Task.CompletedTask;
    }
}
