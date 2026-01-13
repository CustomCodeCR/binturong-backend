using SharedKernel;

namespace Domain.PaymentDetails;

public sealed class PaymentDetail : Entity
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal AppliedAmount { get; set; }

    public Domain.Payments.Payment? Payment { get; set; }
    public Domain.Invoices.Invoice? Invoice { get; set; }
}
