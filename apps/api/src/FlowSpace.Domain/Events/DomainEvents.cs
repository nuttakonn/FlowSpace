using FlowSpace.Domain.Common;

namespace FlowSpace.Domain.Events;

public record WorkspaceCreatedEvent(Guid WorkspaceId, Guid OwnerId) : IDomainEvent;

public record MemberInvitedEvent(Guid WorkspaceId, Guid UserId, string Email) : IDomainEvent;

public record BoardCreatedEvent(Guid BoardId, Guid WorkspaceId) : IDomainEvent;

public record DiagramGeneratedEvent(Guid BoardId, int NodeCount) : IDomainEvent;
