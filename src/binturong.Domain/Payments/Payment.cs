using SharedKernel;

namespace Domain.Payments;

public sealed class Payment : Entity
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public Domain.Clients.Client? Client { get; set; }
    public Domain.PaymentMethods.PaymentMethod? PaymentMethod { get; set; }

    public ICollection<Domain.PaymentDetails.PaymentDetail> Details { get; set; } =
        new List<Domain.PaymentDetails.PaymentDetail>();
    public ICollection<Domain.GatewayTransactions.GatewayTransaction> GatewayTransactions { get; set; } =
        new List<Domain.GatewayTransactions.GatewayTransaction>();

    public void RaiseCreated() =>
        Raise(
            new PaymentCreatedDomainEvent(Id, ClientId, PaymentMethodId, PaymentDate, TotalAmount)
        );

    public void RaiseDeleted() => Raise(new PaymentDeletedDomainEvent(Id));

    public void RaiseApplied(Guid invoiceId, decimal appliedAmount) =>
        Raise(
            new PaymentAppliedToInvoiceDomainEvent(Id, invoiceId, appliedAmount, DateTime.UtcNow)
        );
}
