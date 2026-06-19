using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Events;
using FluentAssertions;
using Xunit;

namespace FlowSpace.UnitTests.Domain.Entities;

public class WorkspaceTests
{
    [Fact]
    public void Workspace_Create_ShouldSetOwnerAsMember_WithOwnerRole()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Main Workspace";
        var ownerId = Guid.NewGuid();

        // Act
        var workspace = Workspace.Create(id, name, ownerId);

        // Assert
        workspace.Id.Should().Be(id);
        workspace.Name.Should().Be(name);
        workspace.OwnerId.Should().Be(ownerId);
        workspace.IsDeleted.Should().BeFalse();
        workspace.Members.Should().ContainSingle(m => m.UserId == ownerId && m.Role == WorkspaceRole.Owner);
    }

    [Fact]
    public void Workspace_AddMember_ShouldAppendToMembersList()
    {
        // Arrange
        var workspace = Workspace.Create(Guid.NewGuid(), "Test Workspace", Guid.NewGuid());
        var newMemberId = Guid.NewGuid();

        // Act
        workspace.AddMember(newMemberId, WorkspaceRole.Editor);

        // Assert
        workspace.Members.Should().HaveCount(2);
        workspace.Members.Should().Contain(m => m.UserId == newMemberId && m.Role == WorkspaceRole.Editor);
    }

    [Fact]
    public void Workspace_Create_ShouldRaiseWorkspaceCreatedDomainEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        // Act
        var workspace = Workspace.Create(id, "Test Workspace", ownerId);

        // Assert
        var domainEvents = workspace.GetDomainEvents();
        domainEvents.Should().ContainSingle();
        var createdEvent = domainEvents.Single().Should().BeOfType<WorkspaceCreatedEvent>().Subject;
        createdEvent.WorkspaceId.Should().Be(id);
        createdEvent.OwnerId.Should().Be(ownerId);
    }
}
