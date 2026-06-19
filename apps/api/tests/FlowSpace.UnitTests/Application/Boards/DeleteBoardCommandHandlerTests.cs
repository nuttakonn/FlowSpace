using FlowSpace.Application.Boards.Commands.DeleteBoard;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Boards;

public class DeleteBoardCommandHandlerTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteBoardCommandHandler _handler;

    public DeleteBoardCommandHandlerTests()
    {
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new DeleteBoardCommandHandler(
            _boardRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteBoard_WhenBoardExists()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = Board.Create(boardId, Guid.NewGuid(), "Board to Delete", "Flowchart", Guid.NewGuid());
        var command = new DeleteBoardCommand(boardId);

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _boardRepositoryMock.Verify(x => x.Delete(board), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBoardNotFound()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var command = new DeleteBoardCommand(boardId);

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Board?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Board.NotFound");

        _boardRepositoryMock.Verify(x => x.Delete(It.IsAny<Board>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
