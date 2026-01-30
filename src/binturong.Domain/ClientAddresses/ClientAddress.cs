using SharedKernel;

namespace Domain.ClientAddresses;

public sealed class ClientAddress : Entity
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }

    public string AddressType { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;
    public string Canton { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Domain.Clients.Client? Client { get; set; }

    // =========================
    // Domain events
    // =========================

    public void RaiseCreated() =>
        Raise(
            new ClientAddressCreatedDomainEvent(
                ClientId,
                Id,
                AddressType,
                AddressLine,
                Province,
                Canton,
                District,
                Notes,
                IsPrimary,
                CreatedAt,
                UpdatedAt
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new ClientAddressUpdatedDomainEvent(
                ClientId,
                Id,
                AddressType,
                AddressLine,
                Province,
                Canton,
                District,
                Notes,
                IsPrimary,
                UpdatedAt
            )
        );

    public void RaiseDeleted() => Raise(new ClientAddressDeletedDomainEvent(ClientId, Id));

    public void RaisePrimarySet() =>
        Raise(new ClientPrimaryAddressSetDomainEvent(ClientId, Id, UpdatedAt));
}
