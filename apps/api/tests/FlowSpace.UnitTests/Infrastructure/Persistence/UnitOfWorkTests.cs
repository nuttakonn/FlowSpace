using FlowSpace.Domain.Common;
using FlowSpace.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Events;

namespace FlowSpace.UnitTests.Infrastructure.Persistence;

public class UnitOfWorkTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _publisherMock = new Mock<IPublisher>();
        _unitOfWork = new UnitOfWork(_dbContext, _publisherMock.Object);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldDispatchDomainEvents_WhenEntitiesHaveEvents()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var workspace = Workspace.Create(workspaceId, "Test Workspace", ownerId);
        
        _dbContext.Workspaces.Add(workspace);

        // Act
        await _unitOfWork.SaveChangesAsync();

        // Assert
        _publisherMock.Verify(
            p => p.Publish(It.Is<IDomainEvent>(e => e is WorkspaceCreatedEvent), It.IsAny<CancellationToken>()),
            Times.Once);

        workspace.GetDomainEvents().Should().BeEmpty("Domain events should be cleared after publishing.");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotRollback_WhenEventPublishingFails()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var workspace = Workspace.Create(workspaceId, "Test Workspace", ownerId);
        
        _dbContext.Workspaces.Add(workspace);

        _publisherMock
            .Setup(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Simulated publishing failure"));

        // Act
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var savedWorkspace = await _dbContext.Workspaces.FindAsync(workspaceId);
        savedWorkspace.Should().NotBeNull("Data should be persisted even if event publishing fails.");
        
        workspace.GetDomainEvents().Should().BeEmpty("Domain events should be cleared even if publishing fails, to prevent infinite loops.");
    }
}
