using System.Net;
using System.Net.Http.Json;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Entities;
using FlowSpace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FluentAssertions;

namespace FlowSpace.IntegrationTests.Persistence;

public class SoftDeleteConsistencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SoftDeleteConsistencyTests(WebApplicationFactory<Program> factory)
    {
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
                    options.UseInMemoryDatabase("SoftDeleteTestsDb");
                });
            });
        });
    }

    [Fact]
    public async Task SoftDeletingBoard_ShouldHideAssociatedNodesAndEdges()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Arrange: Create Workspace, Board, Node, Edge
        var ownerId = Guid.NewGuid();
        var workspace = Workspace.Create(Guid.NewGuid(), "Test Workspace", ownerId);
        
        var board = Board.Create(Guid.NewGuid(), workspace.Id, "Test Board", "Whiteboard", ownerId);
        
        var sourceNode = Node.Create(Guid.NewGuid(), board.Id, "Rectangle", 10, 10);
        var targetNode = Node.Create(Guid.NewGuid(), board.Id, "Rectangle", 100, 100);
        
        var edge = Edge.Create(Guid.NewGuid(), board.Id, sourceNode.Id, targetNode.Id);

        dbContext.Workspaces.Add(workspace);
        dbContext.Boards.Add(board);
        dbContext.Nodes.AddRange(sourceNode, targetNode);
        dbContext.Edges.Add(edge);

        await dbContext.SaveChangesAsync();

        // Act: Soft Delete the Board
        dbContext.Boards.Remove(board); // Remove triggers soft delete due to SaveChangesAsync override
        await dbContext.SaveChangesAsync();

        // Assert: Standard queries should hide the nodes and edges
        var activeNodes = await dbContext.Nodes.Where(n => n.BoardId == board.Id).ToListAsync();
        var activeEdges = await dbContext.Edges.Where(e => e.BoardId == board.Id).ToListAsync();

        activeNodes.Should().BeEmpty("Nodes should be hidden when parent Board is soft-deleted.");
        activeEdges.Should().BeEmpty("Edges should be hidden when parent Board is soft-deleted.");

        // Assert: IgnoreQueryFilters should reveal they still exist in the database
        var rawNodes = await dbContext.Nodes.IgnoreQueryFilters().Where(n => n.BoardId == board.Id).ToListAsync();
        var rawEdges = await dbContext.Edges.IgnoreQueryFilters().Where(e => e.BoardId == board.Id).ToListAsync();

        rawNodes.Should().HaveCount(2, "Nodes should still exist in the database.");
        rawEdges.Should().HaveCount(1, "Edges should still exist in the database.");
    }
}
