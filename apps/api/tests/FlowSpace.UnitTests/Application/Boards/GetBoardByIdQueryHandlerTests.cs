using FlowSpace.Application.Boards.Queries.GetBoardById;
using FlowSpace.Contracts.Boards;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Boards;

public class GetBoardByIdQueryHandlerTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly GetBoardByIdQueryHandler _handler;

    public GetBoardByIdQueryHandlerTests()
    {
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _handler = new GetBoardByIdQueryHandler(_boardRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnBoard_WhenBoardExists()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = Board.Create(boardId, Guid.NewGuid(), "Sample Board", "Flowchart", Guid.NewGuid());
        var query = new GetBoardByIdQuery(boardId);

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(boardId);
        result.Value.Name.Should().Be(board.Name);
        result.Value.Type.Should().Be(board.Type);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBoardNotFound()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var query = new GetBoardByIdQuery(boardId);

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Board?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Board.NotFound");
    }
}
