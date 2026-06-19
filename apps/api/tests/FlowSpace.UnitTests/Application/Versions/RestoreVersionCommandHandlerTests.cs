using FlowSpace.Application.Versions.Commands.RestoreVersion;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using System.Text.Json;
using Xunit;

namespace FlowSpace.UnitTests.Application.Versions;

public class RestoreVersionCommandHandlerTests
{
    private readonly Mock<IBoardVersionRepository> _versionRepositoryMock;
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<INodeRepository> _nodeRepositoryMock;
    private readonly Mock<IEdgeRepository> _edgeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RestoreVersionCommandHandler _handler;

    public RestoreVersionCommandHandlerTests()
    {
        _versionRepositoryMock = new Mock<IBoardVersionRepository>();
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _nodeRepositoryMock = new Mock<INodeRepository>();
        _edgeRepositoryMock = new Mock<IEdgeRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RestoreVersionCommandHandler(
            _versionRepositoryMock.Object,
            _boardRepositoryMock.Object,
            _nodeRepositoryMock.Object,
            _edgeRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRestoreVersion_WhenVersionExists()
    {
        // Arrange
        var versionId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var command = new RestoreVersionCommand(versionId);

        var board = Board.Create(boardId, Guid.NewGuid(), "Test Board", "Flowchart", Guid.NewGuid());
        var version = BoardVersion.Create(boardId, Guid.NewGuid(), Guid.NewGuid(), VersionType.Manual, "v1", "Initial Draft");

        var snapshotNodes = new[]
        {
            new { Id = Guid.NewGuid(), Type = "Rectangle", X = 100.0, Y = 200.0, Width = (double?)150.0, Height = (double?)80.0, Metadata = "{\"text\":\"Node A\"}" }
        };
        var snapshotEdges = new[]
        {
            new { Id = Guid.NewGuid(), SourceNodeId = Guid.NewGuid(), TargetNodeId = Guid.NewGuid(), Metadata = "{}" }
        };

        var snapshot = CanvasSnapshot.Create(boardId, JsonSerializer.Serialize(snapshotNodes), JsonSerializer.Serialize(snapshotEdges), new byte[] { 1, 2, 3 });
        
        // Set Snapshot property using reflection
        typeof(BoardVersion).GetProperty("Snapshot")!.SetValue(version, snapshot);

        var existingNodes = new List<Node>
        {
            Node.Create(Guid.NewGuid(), boardId, "Circle", 50, 50)
        };
        var existingEdges = new List<Edge>
        {
            Edge.Create(Guid.NewGuid(), boardId, Guid.NewGuid(), Guid.NewGuid())
        };

        _versionRepositoryMock.Setup(x => x.GetWithSnapshotAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        _nodeRepositoryMock.Setup(x => x.GetByBoardIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingNodes);

        _edgeRepositoryMock.Setup(x => x.GetByBoardIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEdges);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify deletion of existing elements
        _nodeRepositoryMock.Verify(x => x.Delete(existingNodes[0]), Times.Once);
        _edgeRepositoryMock.Verify(x => x.Delete(existingEdges[0]), Times.Once);

        // Verify addition of snapshot elements
        _nodeRepositoryMock.Verify(x => x.Add(It.Is<Node>(n => 
            n.Id == snapshotNodes[0].Id && 
            n.BoardId == boardId && 
            n.Type == snapshotNodes[0].Type && 
            n.X == snapshotNodes[0].X && 
            n.Y == snapshotNodes[0].Y && 
            n.Width == snapshotNodes[0].Width && 
            n.Height == snapshotNodes[0].Height && 
            n.Metadata == snapshotNodes[0].Metadata
        )), Times.Once);

        _edgeRepositoryMock.Verify(x => x.Add(It.Is<Edge>(e => 
            e.Id == snapshotEdges[0].Id && 
            e.BoardId == boardId && 
            e.SourceNodeId == snapshotEdges[0].SourceNodeId && 
            e.TargetNodeId == snapshotEdges[0].TargetNodeId && 
            e.Metadata == snapshotEdges[0].Metadata
        )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenVersionNotFound()
    {
        // Arrange
        var command = new RestoreVersionCommand(Guid.NewGuid());

        _versionRepositoryMock.Setup(x => x.GetWithSnapshotAsync(command.VersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardVersion?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Version.NotFound");

        _nodeRepositoryMock.Verify(x => x.Delete(It.IsAny<Node>()), Times.Never);
        _edgeRepositoryMock.Verify(x => x.Delete(It.IsAny<Edge>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
