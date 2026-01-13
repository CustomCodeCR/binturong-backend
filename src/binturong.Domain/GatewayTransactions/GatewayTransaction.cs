using SharedKernel;

namespace Domain.GatewayTransactions;

public sealed class GatewayTransaction : Entity
{
    public Guid Id { get; set; }
    public Guid GatewayId { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AuthorizationCode { get; set; } = string.Empty;
    public string GatewayReference { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }

    public Domain.PaymentGatewayConfig.PaymentGatewayConfiguration? Gateway { get; set; }
    public Domain.Payments.Payment? Payment { get; set; }
    public Domain.Invoices.Invoice? Invoice { get; set; }
}
