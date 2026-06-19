using FlowSpace.Application.Common.Abstractions.Authentication;
using FlowSpace.Contracts.Authentication;
using FlowSpace.Domain.Entities;
using FlowSpace.Infrastructure.Persistence;
using FlowSpace.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FlowSpace.IntegrationTests.Authentication;

[Collection("IntegrationTests")]
public class AuthEndpointTests : IClassFixture<FlowSpaceWebFactory>
{
    private readonly FlowSpaceWebFactory _factory;

    public AuthEndpointTests(FlowSpaceWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ShouldReturn200_WithTokens_WhenSeededUserLogsIn()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = "seeded@email.com";
        var password = "Password123!";
        var displayName = "Seeded User";

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var passwordHash = passwordHasher.HashPassword(password);
            var user = User.Create(Guid.NewGuid(), email, passwordHash, displayName);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var request = new LoginRequest(email, password);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        content.Should().NotBeNull();
        content!.Email.Should().Be(email);
        content.DisplayName.Should().Be(displayName);
        content.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturn400_WhenCredentialsAreInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }

        var request = new LoginRequest("invalid-user@email.com", "WrongPassword123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturn404_BecauseEndpointIsRemoved()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new
        {
            Email = "newuser@email.com",
            Password = "Password123!",
            DisplayName = "New User",
            InviteCode = "secret"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
