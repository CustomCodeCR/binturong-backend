using SharedKernel;

namespace Application.Abstractions.Outbox;

public interface IOutboxSerializer
{
    string Serialize(IDomainEvent domainEvent);

    IDomainEvent Deserialize(string payload, string type);
}
