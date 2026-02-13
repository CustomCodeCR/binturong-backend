namespace Application.ReadModels.Sales;

public sealed class PaymentReadModel
{
    public string Id { get; init; } = default!; // "payment:{PaymentId}"
    public Guid PaymentId { get; init; }

    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = default!;

    public Guid PaymentMethodId { get; init; }
    public string PaymentMethodCode { get; init; } = default!;
    public string PaymentMethodDescription { get; init; } = default!;

    public DateTime PaymentDate { get; init; }
    public decimal TotalAmount { get; init; }

    public string? Reference { get; init; }
    public string? Notes { get; init; }

    public IReadOnlyList<PaymentAppliedInvoiceReadModel> AppliedInvoices { get; init; } = [];
}

public sealed class PaymentAppliedInvoiceReadModel
{
    public Guid InvoiceId { get; init; }
    public string? InvoiceConsecutive { get; init; }
    public decimal AppliedAmount { get; init; }
}
