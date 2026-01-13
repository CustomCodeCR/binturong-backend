namespace Application.ReadModels.Payments;

public sealed class GatewayTransactionReadModel
{
    public string Id { get; init; } = default!; // "gw_tx:{GatewayTransactionId}"
    public int GatewayTransactionId { get; init; }

    public int GatewayId { get; init; }
    public string GatewayProvider { get; init; } = default!;
    public string GatewayEnvironment { get; init; } = default!;

    public int? PaymentId { get; init; }
    public int? InvoiceId { get; init; }

    public decimal Amount { get; init; }
    public string Currency { get; init; } = default!;

    public string Status { get; init; } = default!;
    public string? AuthorizationCode { get; init; }
    public string? GatewayReference { get; init; }

    public DateTime TransactionDate { get; init; }

    // Useful for ERP screens
    public string? ClientName { get; init; }
    public string? InvoiceConsecutive { get; init; }
    public string? PaymentMethodCode { get; init; }
}
