using FlowSpace.Application.Versions.Commands.CreateVersion;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Contracts.Versions;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Versions;

public class CreateVersionCommandHandlerTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<IBoardVersionRepository> _versionRepositoryMock;
    private readonly Mock<ICanvasSnapshotRepository> _snapshotRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateVersionCommandHandler _handler;

    public CreateVersionCommandHandlerTests()
    {
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _versionRepositoryMock = new Mock<IBoardVersionRepository>();
        _snapshotRepositoryMock = new Mock<ICanvasSnapshotRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateVersionCommandHandler(
            _boardRepositoryMock.Object,
            _versionRepositoryMock.Object,
            _snapshotRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateVersionWithSnapshot_WhenBoardExists()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CreateVersionCommand(
            boardId,
            userId,
            "v1.0",
            "Initial Draft",
            "[{ \"id\": \"n1\" }]",
            "[]",
            new byte[] { 1, 2, 3 },
            VersionType.Manual);

        var board = Board.Create(boardId, Guid.NewGuid(), "Test Board", "Flowchart", Guid.NewGuid());
        var user = User.Create(userId, "user@email.com", "hash", "John Doe");

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Description.Should().Be(command.Description);
        result.Value.CreatorName.Should().Be(user.DisplayName);
        result.Value.Type.Should().Be((int)VersionType.Manual);

        _snapshotRepositoryMock.Verify(x => x.Add(It.Is<CanvasSnapshot>(snapshot => 
            snapshot.BoardId == boardId && 
            snapshot.NodesData == command.NodesData && 
            snapshot.EdgesData == command.EdgesData && 
            snapshot.YjsState == command.YjsState
        )), Times.Once);

        _versionRepositoryMock.Verify(x => x.Add(It.Is<BoardVersion>(v => 
            v.BoardId == boardId && 
            v.CreatedBy == userId && 
            v.Name == command.Name && 
            v.Description == command.Description && 
            v.Type == VersionType.Manual
        )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBoardNotFound()
    {
        // Arrange
        var command = new CreateVersionCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "v1.0",
            "Initial Draft",
            "[]",
            "[]",
            new byte[] { 1 },
            VersionType.Manual);

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(command.BoardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Board?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Board.NotFound");

        _snapshotRepositoryMock.Verify(x => x.Add(It.IsAny<CanvasSnapshot>()), Times.Never);
        _versionRepositoryMock.Verify(x => x.Add(It.IsAny<BoardVersion>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
