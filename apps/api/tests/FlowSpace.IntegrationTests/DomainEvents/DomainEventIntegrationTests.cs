using System.Net;
using System.Net.Http.Json;
using FlowSpace.Contracts.Workspaces;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Entities;
using FlowSpace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FluentAssertions;

namespace FlowSpace.IntegrationTests.DomainEvents;

public class DomainEventIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DomainEventIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Replace Postgres with InMemory DB for tests
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"JwtSettings:Secret", "this-is-a-dummy-secret-key-that-is-long-enough"}
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(
                    d => d.ServiceType.Name.Contains("DbContextOptions")).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestsDb");
                });
            });
        });
    }

    // Since we need to test internal domain event publishing (which triggers our event handlers),
    // we can either test the API directly (but we don't have a way to assert logs easily),
    // or we can resolve the UnitOfWork and DbContext from the container and trigger it directly.
    // Let's resolve from the container to ensure the DI wiring works.
    
    [Fact]
    public async Task CreateWorkspace_ShouldTriggerDomainEventHandlers_WithoutCrashing()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<FlowSpace.Application.Common.Abstractions.IUnitOfWork>();

        var ownerId = Guid.NewGuid();
        var workspace = Workspace.Create(Guid.NewGuid(), "Test Int Workspace", ownerId);
        
        dbContext.Workspaces.Add(workspace);

        // This will invoke the MediatR publisher inside UnitOfWork,
        // which will find WorkspaceCreatedEventHandler and execute it.
        // If it throws or fails to resolve, this test will fail.
        var act = async () => await unitOfWork.SaveChangesAsync();

        await act.Should().NotThrowAsync();
        
        // Ensure data is saved
        var saved = await dbContext.Workspaces.FindAsync(workspace.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Int Workspace");
    }
}
