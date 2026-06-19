using FlowSpace.Application.Authentication.Commands.Register;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Authentication;
using FlowSpace.Contracts.Authentication;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Authentication;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IInviteCodeService> _inviteCodeServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _inviteCodeServiceMock = new Mock<IInviteCodeService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _inviteCodeServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenEmailIsUniqueAndInviteCodeIsValid()
    {
        // Arrange
        var command = new RegisterCommand("newuser@email.com", "Password123!", "New User", "secret_invite_code");
        
        _inviteCodeServiceMock.Setup(x => x.IsValid(command.InviteCode))
            .Returns(true);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock.Setup(x => x.HashPassword(command.Password))
            .Returns("hashed_password");

        _jwtTokenGeneratorMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("access_token");

        _jwtTokenGeneratorMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
        result.Value.Email.Should().Be(command.Email);
        result.Value.DisplayName.Should().Be(command.DisplayName);

        _userRepositoryMock.Verify(x => x.Add(It.Is<User>(u => u.Email == command.Email && u.DisplayName == command.DisplayName)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInviteCodeIsInvalid()
    {
        // Arrange
        var command = new RegisterCommand("newuser@email.com", "Password123!", "New User", "invalid_invite_code");

        _inviteCodeServiceMock.Setup(x => x.IsValid(command.InviteCode))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidInviteCode");

        _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterCommand("existing@email.com", "Password123!", "Existing User", "secret_invite_code");
        var existingUser = User.Create(Guid.NewGuid(), command.Email, "hashed_password", "Existing User");

        _inviteCodeServiceMock.Setup(x => x.IsValid(command.InviteCode))
            .Returns(true);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.EmailAlreadyExists");

        _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
