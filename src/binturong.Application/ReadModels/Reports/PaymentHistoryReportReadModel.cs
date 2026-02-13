namespace Application.ReadModels.Reports;

public sealed class PaymentHistoryReportReadModel
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }

    public Guid? ClientId { get; init; }
    public Guid? PaymentMethodId { get; init; }
    public string? Search { get; init; }

    public int PaymentsCount { get; init; }
    public decimal TotalCollected { get; init; }
    public decimal AveragePayment { get; init; }

    public IReadOnlyList<PaymentHistoryItemReadModel> Items { get; init; } = [];
    public IReadOnlyList<PaymentHistoryByMethodReadModel> ByMethod { get; init; } = [];
}

public sealed class PaymentHistoryItemReadModel
{
    public Guid PaymentId { get; init; }
    public DateTime PaymentDateUtc { get; init; }

    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;

    public Guid PaymentMethodId { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;

    public decimal TotalAmount { get; init; }
    public string? Reference { get; init; }
    public string? Notes { get; init; }

    public string AppliedInvoices { get; init; } = string.Empty;
}

public sealed class PaymentHistoryByMethodReadModel
{
    public string PaymentMethod { get; init; } = string.Empty;
    public int PaymentsCount { get; init; }
    public decimal TotalCollected { get; init; }
}
