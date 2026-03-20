namespace Application.ReadModels.Services;

public sealed class EmployeeWorkHistoryReadModel
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = default!;
    public IReadOnlyList<EmployeeWorkHistoryEntryReadModel> Entries { get; init; } = [];
}

public sealed class EmployeeWorkHistoryEntryReadModel
{
    public Guid ServiceOrderId { get; init; }
    public string ServiceOrderCode { get; init; } = default!;
    public DateTime ScheduledDate { get; init; }
    public DateTime? ClosedDate { get; init; }
    public string Status { get; init; } = default!;
    public string ClientName { get; init; } = default!;
    public string ServiceAddress { get; init; } = default!;
    public string? Notes { get; init; }

    public IReadOnlyList<string> Services { get; init; } = [];
}
