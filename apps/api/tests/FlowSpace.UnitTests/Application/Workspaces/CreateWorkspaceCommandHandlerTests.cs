using FlowSpace.Application.Workspaces.Commands.CreateWorkspace;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Contracts.Workspaces;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Workspaces;

public class CreateWorkspaceCommandHandlerTests
{
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateWorkspaceCommandHandler _handler;

    public CreateWorkspaceCommandHandlerTests()
    {
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateWorkspaceCommandHandler(
            _workspaceRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateWorkspace_AndAddOwnerAsMember()
    {
        // Arrange
        var command = new CreateWorkspaceCommand("Test Workspace", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _workspaceRepositoryMock.Verify(x => x.Add(It.Is<Workspace>(w =>
            w.Name == command.Name &&
            w.OwnerId == command.OwnerId &&
            w.Members.Any(m => m.UserId == command.OwnerId && m.Role == WorkspaceRole.Owner)
        )), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnWorkspaceResponse_WithCorrectData()
    {
        // Arrange
        var command = new CreateWorkspaceCommand("Test Workspace", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.OwnerId.Should().Be(command.OwnerId);
        result.Value.Id.Should().NotBeEmpty();
    }
}
