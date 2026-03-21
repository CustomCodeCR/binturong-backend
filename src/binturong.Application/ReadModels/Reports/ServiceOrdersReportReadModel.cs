namespace Application.ReadModels.Reports;

public sealed class ServiceOrdersReportReadModel
{
    public DateTime FromUtc { get; init; }
    public DateTime ToUtc { get; init; }

    public int CompletedCount { get; init; }
    public int PendingCount { get; init; }
    public int CanceledCount { get; init; }

    public bool HasData { get; init; }
    public string? Message { get; init; }

    public IReadOnlyList<ServiceOrdersReportItemReadModel> Items { get; init; } = [];
}

public sealed class ServiceOrdersReportItemReadModel
{
    public Guid ServiceOrderId { get; init; }
    public string Code { get; init; } = default!;
    public string ClientName { get; init; } = default!;
    public DateTime ScheduledDate { get; init; }
    public string Status { get; init; } = default!;
    public string ServiceAddress { get; init; } = default!;
    public IReadOnlyList<string> Technicians { get; init; } = [];
    public IReadOnlyList<string> Services { get; init; } = [];
}
