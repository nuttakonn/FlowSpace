using FlowSpace.Application.Boards.Commands.CreateBoard;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Contracts.Boards;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Boards;

public class CreateBoardCommandHandlerTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateBoardCommandHandler _handler;

    public CreateBoardCommandHandlerTests()
    {
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateBoardCommandHandler(
            _boardRepositoryMock.Object,
            _workspaceRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateBoard_WhenWorkspaceExists()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var command = new CreateBoardCommand(workspaceId, "New Board", "Flowchart", creatorId);
        var workspace = Workspace.Create(workspaceId, "Test Workspace", creatorId);

        _workspaceRepositoryMock.Setup(x => x.GetByIdAsync(workspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Type.Should().Be(command.Type);
        result.Value.WorkspaceId.Should().Be(command.WorkspaceId);

        _boardRepositoryMock.Verify(x => x.Add(It.Is<Board>(b => b.Name == command.Name && b.WorkspaceId == command.WorkspaceId && b.Type == command.Type)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenWorkspaceNotFound()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var command = new CreateBoardCommand(workspaceId, "New Board", "Flowchart", Guid.NewGuid());

        _workspaceRepositoryMock.Setup(x => x.GetByIdAsync(workspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workspace?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workspace.NotFound");

        _boardRepositoryMock.Verify(x => x.Add(It.IsAny<Board>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
