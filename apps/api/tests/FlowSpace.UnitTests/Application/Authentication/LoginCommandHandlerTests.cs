using FlowSpace.Application.Authentication.Commands.Login;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Authentication;
using FlowSpace.Contracts.Authentication;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowSpace.UnitTests.Application.Authentication;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginCommand("test@email.com", "Password123!");
        var user = User.Create(Guid.NewGuid(), command.Email, "hashed_password", "Test User");
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _jwtTokenGeneratorMock.Setup(x => x.GenerateToken(user))
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
        result.Value.Email.Should().Be(user.Email);
        result.Value.DisplayName.Should().Be(user.DisplayName);

        user.RefreshTokens.Should().ContainSingle();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@email.com", "Password123!");
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPasswordIsWrong()
    {
        // Arrange
        var command = new LoginCommand("test@email.com", "WrongPassword!");
        var user = User.Create(Guid.NewGuid(), command.Email, "hashed_password", "Test User");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
