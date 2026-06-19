using FlowSpace.Application.Sharing.Commands.CreateShareLink;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Contracts.Sharing;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Sharing;

public class CreateShareLinkCommandHandlerTests
{
    private readonly Mock<IBoardRepository> _boardRepositoryMock;
    private readonly Mock<IBoardShareLinkRepository> _shareLinkRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly CreateShareLinkCommandHandler _handler;

    public CreateShareLinkCommandHandlerTests()
    {
        _boardRepositoryMock = new Mock<IBoardRepository>();
        _shareLinkRepositoryMock = new Mock<IBoardShareLinkRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["FrontendUrl"]).Returns("http://localhost:3000");

        _handler = new CreateShareLinkCommandHandler(
            _boardRepositoryMock.Object,
            _shareLinkRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateShareLink_WhenBoardExists()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var command = new CreateShareLinkCommand(boardId, BoardRole.Editor, DateTime.UtcNow.AddDays(1));
        var board = Board.Create(boardId, Guid.NewGuid(), "Test Board", "Flowchart", Guid.NewGuid());

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Role.Should().Be((int)BoardRole.Editor);
        result.Value.ExpiresAt.Should().Be(command.ExpiresAt);
        result.Value.IsRevoked.Should().BeFalse();
        result.Value.Url.Should().Contain($"/shared/{result.Value.Token}");

        _shareLinkRepositoryMock.Verify(x => x.Add(It.Is<BoardShareLink>(link => 
            link.BoardId == boardId && 
            link.Role == BoardRole.Editor && 
            link.ExpiresAt == command.ExpiresAt
        )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBoardNotFound()
    {
        // Arrange
        var command = new CreateShareLinkCommand(Guid.NewGuid(), BoardRole.Viewer);

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(command.BoardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Board?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Board.NotFound");
        _shareLinkRepositoryMock.Verify(x => x.Add(It.IsAny<BoardShareLink>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueToken()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var command = new CreateShareLinkCommand(boardId, BoardRole.Viewer);
        var board = Board.Create(boardId, Guid.NewGuid(), "Test Board", "Flowchart", Guid.NewGuid());

        _boardRepositoryMock.Setup(x => x.GetByIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        // Act
        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Token.Should().NotBeNullOrEmpty();
        result2.Value.Token.Should().NotBeNullOrEmpty();
        result1.Value.Token.Should().NotBe(result2.Value.Token);
    }
}
