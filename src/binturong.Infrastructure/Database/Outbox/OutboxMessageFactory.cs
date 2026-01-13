using Application.Abstractions.Outbox;
using Domain.OutboxMessages;
using SharedKernel;

namespace Infrastructure.Database.Outbox;

internal sealed class OutboxMessageFactory : IOutboxMessageFactory
{
    private readonly IOutboxSerializer _serializer;

    public OutboxMessageFactory(IOutboxSerializer serializer)
    {
        _serializer = serializer;
    }

    public OutboxMessage Create(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var payload = _serializer.Serialize(domainEvent);
        var type =
            domainEvent.GetType().AssemblyQualifiedName
            ?? domainEvent.GetType().FullName
            ?? domainEvent.GetType().Name;

        // Ajustá nombres si tu entidad difiere
        return new OutboxMessage
        {
            OccurredAt = DateTime.UtcNow,
            Type = type,
            PayloadJson = payload,
            Status = "PENDING",
            Attempts = 0,
            LastError = null,
            NextAttemptAt = null,
        };
    }
}
