using SharedKernel;

namespace Application.Abstractions.Projections;

public interface IProjectionDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
