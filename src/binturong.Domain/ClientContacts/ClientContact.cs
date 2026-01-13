using SharedKernel;

namespace Domain.ClientContacts;

public sealed class ClientContact : Entity
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public Domain.Clients.Client? Client { get; set; }
}
