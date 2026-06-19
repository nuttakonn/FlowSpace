using FlowSpace.Application.Boards.Commands.UpdateBoard;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Boards;

public class UpdateBoardCommandHandlerTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateBoardCommandHandler _handler;

    public UpdateBoardCommandHandlerTests()
    {
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new UpdateBoardCommandHandler(
            _boardRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateBoard_WhenBoardExists()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = Board.Create(boardId, Guid.NewGuid(), "Old Name", "Flowchart", Guid.NewGuid());
        var command = new UpdateBoardCommand(boardId, "New Name");

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        board.Name.Should().Be(command.Name);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBoardNotFound()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var command = new UpdateBoardCommand(boardId, "New Name");

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Board?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Board.NotFound");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
