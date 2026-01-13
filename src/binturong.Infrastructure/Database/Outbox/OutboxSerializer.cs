using System.Text.Json;
using Application.Abstractions.Outbox;
using SharedKernel;

namespace Infrastructure.Database.Outbox;

internal sealed class OutboxSerializer : IOutboxSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    public string Serialize(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        return JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), Options);
    }

    public IDomainEvent Deserialize(string payload, string type)
    {
        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Outbox payload is empty.", nameof(payload));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Outbox type is empty.", nameof(type));

        var eventType = ResolveType(type);

        if (!typeof(IDomainEvent).IsAssignableFrom(eventType))
            throw new InvalidOperationException($"Type '{type}' is not an IDomainEvent.");

        var deserialized = JsonSerializer.Deserialize(payload, eventType, Options);

        return deserialized as IDomainEvent
            ?? throw new InvalidOperationException(
                $"Failed to deserialize outbox payload as '{type}'."
            );
    }

    private static Type ResolveType(string type)
    {
        // 1) Preferred: AssemblyQualifiedName works directly
        var resolved = Type.GetType(type, throwOnError: false, ignoreCase: false);
        if (resolved is not null)
            return resolved;

        // 2) Fallback: search loaded assemblies by FullName
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            resolved = asm.GetType(type, throwOnError: false, ignoreCase: false);
            if (resolved is not null)
                return resolved;
        }

        throw new InvalidOperationException(
            $"Unable to resolve event type '{type}'. "
                + "Store the event type as AssemblyQualifiedName or ensure the assembly is loaded."
        );
    }
}
