using FlowSpace.Application.Common.Abstractions.Interop;
using FlowSpace.Application.Common.Abstractions.Authentication;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Entities;
using FlowSpace.Infrastructure.Persistence;
using FlowSpace.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using Xunit;

namespace FlowSpace.IntegrationTests.Boards;

[Collection("IntegrationTests")]
public class ExportEndpointTests : IClassFixture<FlowSpaceWebFactory>
{
    private readonly FlowSpaceWebFactory _factory;

    public ExportEndpointTests(FlowSpaceWebFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateAuthenticatedClientForFactory(WebApplicationFactory<Program> factory, string email, string displayName, out User user)
    {
        var tempUser = User.Create(Guid.NewGuid(), email, "password_hash", displayName);
        user = tempUser;

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        dbContext.Users.Add(tempUser);
        dbContext.SaveChanges();

        var jwtGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var token = jwtGenerator.GenerateToken(tempUser);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private class FakeExportService : IExportService
    {
        public Task<byte[]> ExportToPngAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", string shareToken = "", CancellationToken cancellationToken = default)
            => Task.FromResult(new byte[] { 1, 2, 3 });

        public Task<byte[]> ExportToJpgAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", string shareToken = "", CancellationToken cancellationToken = default)
            => Task.FromResult(new byte[] { 4, 5, 6 });

        public Task<byte[]> ExportToPdfAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", string shareToken = "", CancellationToken cancellationToken = default)
            => Task.FromResult(new byte[] { 7, 8, 9 });

        public Task<string> ExportToSvgAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", string shareToken = "", CancellationToken cancellationToken = default)
            => Task.FromResult("<svg></svg>");
    }

    [Fact]
    public async Task ExportBoard_ShouldReturnFile_WhenAuthenticatedAndFormatIsPng()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }

        var email = "owner@email.com";
        var displayName = "Workspace Owner";

        var clientFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IExportService>();
                services.AddScoped<IExportService, FakeExportService>();
            });
        });

        User user;
        var client = CreateAuthenticatedClientForFactory(clientFactory, email, displayName, out user);

        var boardId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();

        using (var scope = clientFactory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Setup Workspace and Board
            var workspace = Workspace.Create(workspaceId, "My Work", user.Id);
            dbContext.Workspaces.Add(workspace);

            var board = Board.Create(boardId, workspaceId, "Design Whiteboard", "Whiteboard", user.Id);
            dbContext.Boards.Add(board);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/v1/interop/boards/{boardId}/export?format=png");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("image/png");
        
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().Equal(new byte[] { 1, 2, 3 });
    }

    [Fact]
    public async Task ExportBoard_ShouldReturnJson_WhenFormatIsFlowSpace()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }

        var email = "owner@email.com";
        User user;
        var client = CreateAuthenticatedClientForFactory(_factory, email, "Owner", out user);

        var boardId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var workspace = Workspace.Create(workspaceId, "My Work", user.Id);
            dbContext.Workspaces.Add(workspace);

            var board = Board.Create(boardId, workspaceId, "Design Whiteboard", "Whiteboard", user.Id);
            dbContext.Boards.Add(board);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/v1/interop/boards/{boardId}/export?format=flowspace");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var jsonContent = await response.Content.ReadAsStringAsync();
        jsonContent.Should().Contain("Design Whiteboard");
    }
}
