using FlowSpace.Application.Common.Abstractions.Authentication;
using FlowSpace.Domain.Entities;
using FlowSpace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace FlowSpace.IntegrationTests.Common;

public class FlowSpaceWebFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JwtSettings:Secret", "test-secret-key-that-is-at-least-32-chars" },
                { "JwtSettings:Issuer", "FlowSpace" },
                { "JwtSettings:Audience", "FlowSpaceClient" },
                { "TeamSeeder:Enabled", "false" },
                { "ApplyMigrationsOnStartup", "false" },
                { "ConnectionStrings:DefaultConnection", "InMemoryDbForTesting" },
                { "INVITE_CODE", "your-secret-team-code" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove EF Core registrations to prevent provider conflicts
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(ApplicationDbContext)
            ).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Create a separate internal service provider for InMemory EF services
            var internalServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting")
                       .UseInternalServiceProvider(internalServiceProvider);
            });
        });
    }

    public HttpClient CreateAuthenticatedClient(string email, string displayName)
    {
        var user = User.Create(Guid.NewGuid(), email, "password_hash", displayName);

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        var jwtGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var token = jwtGenerator.GenerateToken(user);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
