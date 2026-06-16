using FlowSpace.Application.Boards.Commands.UpdateWhiteboard;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using Moq;
using FluentAssertions;

namespace FlowSpace.UnitTests.Application.Boards;

public class WhiteboardPersistenceTests
{
    private readonly Mock<INodeRepository> _nodeRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly UpdateWhiteboardCommandHandler _handler;

    public WhiteboardPersistenceTests()
    {
        _nodeRepoMock = new Mock<INodeRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _handler = new UpdateWhiteboardCommandHandler(_nodeRepoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddNode_WhenRecordIsNew()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var records = new Dictionary<string, object>
        {
            { "shape:rect1", new { id = "shape:rect1", type = "geo", x = 10, y = 20 } }
        };
        var command = new UpdateWhiteboardCommand(boardId, records);

        _nodeRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Node?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _nodeRepoMock.Verify(x => x.Add(It.IsAny<Node>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
