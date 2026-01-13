namespace Application.ReadModels.Payments;

public sealed class PaymentGatewayConfigReadModel
{
    public string Id { get; init; } = default!; // "gateway:{GatewayId}"
    public int GatewayId { get; init; }

    public string Provider { get; init; } = default!;
    public string PublicKey { get; init; } = default!;
    public string Environment { get; init; } = default!; // DEV / STAGING / PROD

    // Reference to secret in AWS Secrets Manager / SSM
    public string SecretRef { get; init; } = default!;

    public bool IsActive { get; init; }
}
