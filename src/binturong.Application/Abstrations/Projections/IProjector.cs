using SharedKernel;

namespace Application.Abstractions.Projections;

public interface IProjector<in TEvent>
    where TEvent : IDomainEvent
{
    Task ProjectAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
