using FlowSpace.Application.Sharing.Commands.RevokeShareLink;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Common;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Sharing;

public class RevokeShareLinkCommandHandlerTests
{
    private readonly Mock<IBoardShareLinkRepository> _shareLinkRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RevokeShareLinkCommandHandler _handler;

    public RevokeShareLinkCommandHandlerTests()
    {
        _shareLinkRepositoryMock = new Mock<IBoardShareLinkRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RevokeShareLinkCommandHandler(
            _shareLinkRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRevokeLink_WhenLinkExists()
    {
        // Arrange
        var linkId = Guid.NewGuid();
        var command = new RevokeShareLinkCommand(linkId);
        var link = BoardShareLink.Create(Guid.NewGuid(), BoardRole.Viewer);
        
        // Use reflection to set the private Id field of the entity to linkId
        typeof(Entity).GetProperty("Id")!.SetValue(link, linkId);

        _shareLinkRepositoryMock.Setup(x => x.GetByIdAsync(linkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(link);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        link.IsRevoked.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLinkNotFound()
    {
        // Arrange
        var command = new RevokeShareLinkCommand(Guid.NewGuid());

        _shareLinkRepositoryMock.Setup(x => x.GetByIdAsync(command.LinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardShareLink?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ShareLink.NotFound");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
