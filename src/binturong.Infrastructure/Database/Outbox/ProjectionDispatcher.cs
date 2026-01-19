using Application.Abstractions.Projections;
using SharedKernel;

namespace Infrastructure.Database.Outbox;

internal sealed class ProjectionDispatcher : IProjectionDispatcher
{
    public Task DispatchAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    ) => Task.CompletedTask;
}
