using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Authorization;
using FluentAssertions;

namespace FlowSpace.UnitTests.Domain.Entities;

public class BoardShareLinkTests
{
    [Fact]
    public void Create_ShouldGenerateValidToken()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var role = BoardRole.Viewer;

        // Act
        var link = BoardShareLink.Create(boardId, role);

        // Assert
        link.Token.Should().NotBeEmpty();
        link.Token.Length.Should().Be(32);
        link.Role.Should().Be(role);
        link.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenRevoked()
    {
        // Arrange
        var link = BoardShareLink.Create(Guid.NewGuid(), BoardRole.Viewer);
        link.Revoke();

        // Act
        var isValid = link.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenExpired()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(-1);
        var link = BoardShareLink.Create(Guid.NewGuid(), BoardRole.Viewer, expiresAt);

        // Act
        var isValid = link.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenNotExpiredOrRevoked()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var link = BoardShareLink.Create(Guid.NewGuid(), BoardRole.Viewer, expiresAt);

        // Act
        var isValid = link.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }
}
