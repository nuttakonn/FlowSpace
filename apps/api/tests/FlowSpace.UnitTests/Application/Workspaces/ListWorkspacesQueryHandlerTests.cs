using FlowSpace.Application.Workspaces.Queries.ListWorkspaces;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Workspaces;

public class ListWorkspacesQueryHandlerTests
{
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly ListWorkspacesQueryHandler _handler;

    public ListWorkspacesQueryHandlerTests()
    {
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
        _handler = new ListWorkspacesQueryHandler(_workspaceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnWorkspaces_BelongingToCurrentUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new ListWorkspacesQuery(userId);

        var workspaces = new List<Workspace>
        {
            Workspace.Create(Guid.NewGuid(), "Workspace 1", userId),
            Workspace.Create(Guid.NewGuid(), "Workspace 2", userId)
        };

        _workspaceRepositoryMock.Setup(x => x.ListByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspaces);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        
        result.Value[0].Id.Should().Be(workspaces[0].Id);
        result.Value[0].Name.Should().Be(workspaces[0].Name);
        result.Value[0].OwnerId.Should().Be(workspaces[0].OwnerId);
        
        result.Value[1].Id.Should().Be(workspaces[1].Id);
        result.Value[1].Name.Should().Be(workspaces[1].Name);
        result.Value[1].OwnerId.Should().Be(workspaces[1].OwnerId);
    }
}
