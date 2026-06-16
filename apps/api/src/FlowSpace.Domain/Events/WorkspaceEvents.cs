using FlowSpace.Domain.Common;
using FlowSpace.Domain.Authorization;

namespace FlowSpace.Domain.Events;

public record WorkspaceMemberRemovedEvent(Guid WorkspaceId, Guid UserId) : IDomainEvent;

public record WorkspaceMemberRoleChangedEvent(Guid WorkspaceId, Guid UserId, WorkspaceRole NewRole) : IDomainEvent;
