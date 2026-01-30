using SharedKernel;

namespace Domain.ClientContacts;

public sealed class ClientContact : Entity
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? JobTitle { get; set; }

    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

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
            new ClientContactCreatedDomainEvent(
                ClientId,
                Id,
                Name,
                JobTitle,
                Email,
                Phone,
                IsPrimary,
                CreatedAt,
                UpdatedAt
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new ClientContactUpdatedDomainEvent(
                ClientId,
                Id,
                Name,
                JobTitle,
                Email,
                Phone,
                IsPrimary,
                UpdatedAt
            )
        );

    public void RaiseDeleted() => Raise(new ClientContactDeletedDomainEvent(ClientId, Id));

    public void RaisePrimarySet() =>
        Raise(new ClientPrimaryContactSetDomainEvent(ClientId, Id, UpdatedAt));
}
