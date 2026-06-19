using FlowSpace.Contracts.Boards;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Entities;
using FlowSpace.Infrastructure.Persistence;
using FlowSpace.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FlowSpace.IntegrationTests.Boards;

[Collection("IntegrationTests")]
public class BoardsEndpointTests : IClassFixture<FlowSpaceWebFactory>
{
    private readonly FlowSpaceWebFactory _factory;

    public BoardsEndpointTests(FlowSpaceWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBoard_ShouldReturn201_WhenAuthenticated()
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
        var client = _factory.CreateAuthenticatedClient(email, displayName);

        var workspaceId = Guid.NewGuid();
        var workspaceName = "Primary Workspace";

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            user.Should().NotBeNull();

            var workspace = Workspace.Create(workspaceId, workspaceName, user!.Id);
            dbContext.Workspaces.Add(workspace);
            await dbContext.SaveChangesAsync();
        }

        var request = new CreateBoardRequest("New Project Board", "Flowchart");

        // Act
        var response = await client.PostAsJsonAsync($"/api/v1/workspaces/{workspaceId}/boards", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var boardResponse = await response.Content.ReadFromJsonAsync<BoardResponse>();
        boardResponse.Should().NotBeNull();
        boardResponse!.Name.Should().Be(request.Name);
        boardResponse.Type.Should().Be(request.Type);
        boardResponse.WorkspaceId.Should().Be(workspaceId);
    }

    [Fact]
    public async Task CreateBoard_ShouldReturn401_WhenNotAuthenticated()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }

        var client = _factory.CreateClient();
        var request = new CreateBoardRequest("Unauthorized Board", "Whiteboard");

        // Act
        var response = await client.PostAsJsonAsync($"/api/v1/workspaces/{Guid.NewGuid()}/boards", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBoard_ShouldReturn404_WhenNotFound()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }

        var email = "editor@email.com";
        var client = _factory.CreateAuthenticatedClient(email, "Editor User");
        var nonExistentBoardId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            user.Should().NotBeNull();

            // Seed a workspace with the same ID as the board ID so that workspace permission check passes
            var workspace = Workspace.Create(nonExistentBoardId, "Dummy Workspace for Perms", user!.Id);
            dbContext.Workspaces.Add(workspace);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/v1/boards/{nonExistentBoardId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListBoards_ShouldReturn200_WhenAuthenticated()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }

        var email = "member@email.com";
        var client = _factory.CreateAuthenticatedClient(email, "Member User");
        var workspaceId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            user.Should().NotBeNull();

            var workspace = Workspace.Create(workspaceId, "Co-working Space", user!.Id);
            dbContext.Workspaces.Add(workspace);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/v1/workspaces/{workspaceId}/boards");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var list = await response.Content.ReadFromJsonAsync<List<BoardResponse>>();
        list.Should().NotBeNull();
    }
}
