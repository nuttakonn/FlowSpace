using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Domain.Common;
using MediatR;

namespace FlowSpace.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublisher _publisher;

    public UnitOfWork(ApplicationDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Collect Domain Events
        var entitiesWithDomainEvents = _dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.GetDomainEvents().Any())
            .Select(entry => entry.Entity)
            .ToList();

        var domainEvents = entitiesWithDomainEvents
            .SelectMany(entity => entity.GetDomainEvents())
            .ToList();

        // 4. Clear Events (Do this before publishing to avoid infinite loops if handlers trigger saves)
        entitiesWithDomainEvents.ForEach(entity => entity.ClearDomainEvents());

        // 2. Persist Transaction
        var result = await _dbContext.SaveChangesAsync(cancellationToken);

        // 3. Publish Events
        foreach (var domainEvent in domainEvents)
        {
            // If publishing fails, it will throw, but transaction is already committed.
            // As per requirements: "Event publishing failure must not rollback committed data."
            // So we can wrap in try-catch or let it fail gracefully (e.g., logging).
            // Let's use INotification for MediatR publishing.
            // Wait, our IDomainEvent interface needs to inherit from INotification for MediatR.
            
            try
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log failure, but don't rethrow to avoid rolling back the user's action
                // (though EF save is already done, an unhandled exception would result in a 500 error for the client)
                // Assuming we have a logger, or we just let it continue for now.
                Console.WriteLine($"Failed to publish domain event {domainEvent.GetType().Name}: {ex.Message}");
            }
        }

        return result;
    }
}
