using FlowSpace.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace FlowSpace.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void User_Create_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "test@email.com";
        var passwordHash = "hashed_pass";
        var displayName = "Test User";
        var avatarUrl = "http://avatar.com/1";

        // Act
        var user = User.Create(id, email, passwordHash, displayName, avatarUrl);

        // Assert
        user.Id.Should().Be(id);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.DisplayName.Should().Be(displayName);
        user.AvatarUrl.Should().Be(avatarUrl);
        user.RefreshTokens.Should().BeEmpty();
    }

    [Fact]
    public void User_AddRefreshToken_ShouldAppendToken()
    {
        // Arrange
        var user = User.Create(Guid.NewGuid(), "test@email.com", "hash", "Test User");
        var tokenHash = "token_hash_123";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        user.AddRefreshToken(tokenHash, expiresAt);

        // Assert
        user.RefreshTokens.Should().ContainSingle();
        user.RefreshTokens.Should().Contain(rt => rt.TokenHash == tokenHash);
    }

    [Fact]
    public void User_AddRefreshToken_ShouldSetCorrectExpiry()
    {
        // Arrange
        var user = User.Create(Guid.NewGuid(), "test@email.com", "hash", "Test User");
        var tokenHash = "token_hash_456";
        var expiresAt = DateTime.UtcNow.AddHours(2);

        // Act
        user.AddRefreshToken(tokenHash, expiresAt);

        // Assert
        user.RefreshTokens.Should().ContainSingle();
        var token = user.RefreshTokens.Single();
        token.ExpiresAt.Should().Be(expiresAt);
    }
}
