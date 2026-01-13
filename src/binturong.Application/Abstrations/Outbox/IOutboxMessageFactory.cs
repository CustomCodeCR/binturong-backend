using Domain.OutboxMessages;
using SharedKernel;

namespace Application.Abstractions.Outbox;

public interface IOutboxMessageFactory
{
    OutboxMessage Create(IDomainEvent domainEvent);
}
