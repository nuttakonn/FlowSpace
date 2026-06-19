using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Events;
using FluentAssertions;
using Xunit;

namespace FlowSpace.UnitTests.Domain.Entities;

public class BoardTests
{
    [Fact]
    public void Board_Create_ShouldSetAllProperties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var name = "Test Board";
        var type = "Flowchart";
        var creatorId = Guid.NewGuid();

        // Act
        var board = Board.Create(id, workspaceId, name, type, creatorId);

        // Assert
        board.Id.Should().Be(id);
        board.WorkspaceId.Should().Be(workspaceId);
        board.Name.Should().Be(name);
        board.Type.Should().Be(type);
        board.Visibility.Should().Be(BoardVisibility.Private);
        board.IsDeleted.Should().BeFalse();
        board.Permissions.Should().ContainSingle(p => p.UserId == creatorId && p.Role == BoardRole.Owner);
    }

    [Fact]
    public void Board_Update_ShouldChangeName()
    {
        // Arrange
        var board = Board.Create(Guid.NewGuid(), Guid.NewGuid(), "Old Name", "Whiteboard", Guid.NewGuid());

        // Act
        board.Update("New Name");

        // Assert
        board.Name.Should().Be("New Name");
    }

    [Fact]
    public void Board_Create_ShouldRaiseBoardCreatedDomainEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        // Act
        var board = Board.Create(id, workspaceId, "Test Board", "Flowchart", creatorId);

        // Assert
        var domainEvents = board.GetDomainEvents();
        domainEvents.Should().ContainSingle();
        var createdEvent = domainEvents.Single().Should().BeOfType<BoardCreatedEvent>().Subject;
        createdEvent.BoardId.Should().Be(id);
        createdEvent.WorkspaceId.Should().Be(workspaceId);
    }
}
