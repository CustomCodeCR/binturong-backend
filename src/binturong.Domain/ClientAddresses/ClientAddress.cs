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
    public string Notes { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public Domain.Clients.Client? Client { get; set; }
}
