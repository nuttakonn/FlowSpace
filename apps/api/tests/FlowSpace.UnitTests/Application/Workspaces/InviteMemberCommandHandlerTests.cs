using FlowSpace.Application.Workspaces.Commands.InviteMember;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Workspaces;

public class InviteMemberCommandHandlerTests
{
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly InviteMemberCommandHandler _handler;

    public InviteMemberCommandHandlerTests()
    {
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new InviteMemberCommandHandler(
            _workspaceRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddMember_WhenWorkspaceExists()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var email = "newmember@email.com";
        var role = WorkspaceRole.Editor;
        var command = new InviteMemberCommand(workspaceId, email, role);

        var workspace = Workspace.Create(workspaceId, "Test Workspace", Guid.NewGuid());
        var user = User.Create(Guid.NewGuid(), email, "password_hash", "New Member");

        _workspaceRepositoryMock.Setup(x => x.GetByIdAsync(workspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        workspace.Members.Should().Contain(m => m.UserId == user.Id && m.Role == role);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenWorkspaceNotFound()
    {
        // Arrange
        var command = new InviteMemberCommand(Guid.NewGuid(), "newmember@email.com", WorkspaceRole.Editor);

        _workspaceRepositoryMock.Setup(x => x.GetByIdAsync(command.WorkspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workspace?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workspace.NotFound");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMemberAlreadyExists()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var email = "existingmember@email.com";
        var command = new InviteMemberCommand(workspaceId, email, WorkspaceRole.Editor);

        var workspace = Workspace.Create(workspaceId, "Test Workspace", Guid.NewGuid());
        var user = User.Create(Guid.NewGuid(), email, "password_hash", "Existing Member");
        
        // Add member beforehand
        workspace.AddMember(user.Id, WorkspaceRole.Editor);

        _workspaceRepositoryMock.Setup(x => x.GetByIdAsync(workspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workspace.AlreadyMember");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
