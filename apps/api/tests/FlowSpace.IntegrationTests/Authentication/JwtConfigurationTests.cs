using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FluentAssertions;
using FlowSpace.Infrastructure.Authentication;

namespace FlowSpace.IntegrationTests.Authentication;

public class JwtConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public JwtConfigurationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Startup_ShouldFail_WhenJwtSecretIsMissing()
    {
        // Arrange
        var brokenFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Ensure no secret is provided
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"JwtSettings:Secret", null}
                });
            });
        });

        // Act
        var act = () => brokenFactory.Services.GetRequiredService<IOptions<JwtSettings>>().Value;

        // Assert
        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*JWT Secret is required*");
    }

    [Fact]
    public void Startup_ShouldSucceed_WhenJwtSecretIsProvided()
    {
        // Arrange
        var validFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"JwtSettings:Secret", "this-is-a-valid-secret-key-that-is-at-least-32-chars"}
                });
            });
        });

        // Act
        var settings = validFactory.Services.GetRequiredService<IOptions<JwtSettings>>().Value;

        // Assert
        settings.Should().NotBeNull();
        settings.Secret.Should().Be("this-is-a-valid-secret-key-that-is-at-least-32-chars");
    }
}
