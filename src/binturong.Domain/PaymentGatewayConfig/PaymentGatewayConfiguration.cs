using SharedKernel;

namespace Domain.PaymentGatewayConfig;

public sealed class PaymentGatewayConfiguration : Entity
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string SecretRef { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public ICollection<Domain.GatewayTransactions.GatewayTransaction> Transactions { get; set; } =
        new List<Domain.GatewayTransactions.GatewayTransaction>();
}
