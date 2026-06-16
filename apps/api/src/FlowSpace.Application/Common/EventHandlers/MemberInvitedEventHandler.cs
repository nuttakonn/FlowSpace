using FlowSpace.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowSpace.Application.Common.EventHandlers;

public class MemberInvitedEventHandler : INotificationHandler<MemberInvitedEvent>
{
    private readonly ILogger<MemberInvitedEventHandler> _logger;

    public MemberInvitedEventHandler(ILogger<MemberInvitedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(MemberInvitedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: User {UserId} ({Email}) was invited to Workspace {WorkspaceId}.", notification.UserId, notification.Email, notification.WorkspaceId);
        
        // Future: Send email invitation via an email service.
        
        return Task.CompletedTask;
    }
}
