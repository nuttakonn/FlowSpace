using FlowSpace.Application.Workspaces.Commands.RemoveMember;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Workspaces;

public class RemoveMemberCommandHandlerTests
{
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveMemberCommandHandler _handler;

    public RemoveMemberCommandHandlerTests()
    {
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RemoveMemberCommandHandler(
            _workspaceRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRemoveMember_WhenMemberExists()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RemoveMemberCommand(workspaceId, userId);

        var workspace = Workspace.Create(workspaceId, "Test Workspace", Guid.NewGuid());
        workspace.AddMember(userId, WorkspaceRole.Editor);

        _workspaceRepositoryMock.Setup(x => x.GetByIdAsync(workspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        workspace.Members.Should().NotContain(m => m.UserId == userId);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMemberNotFound()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RemoveMemberCommand(workspaceId, userId);

        var workspace = Workspace.Create(workspaceId, "Test Workspace", Guid.NewGuid());

        _workspaceRepositoryMock.Setup(x => x.GetByIdAsync(workspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workspace.MemberNotFound");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
