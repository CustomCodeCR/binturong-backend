public sealed class ContractReadModel
{
    public string Id { get; init; } = default!;
    public Guid ContractId { get; init; }
    public string Code { get; init; } = default!;

    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = default!;

    public Guid? QuoteId { get; init; }
    public Guid? SalesOrderId { get; init; }

    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }

    public string Status { get; init; } = default!;
    public string? Description { get; init; }
    public string? Notes { get; init; }

    public Guid? ResponsibleUserId { get; init; }
    public bool AutoRenewEnabled { get; init; }
    public int AutoRenewEveryDays { get; init; }
    public int ExpiryNoticeDays { get; init; }
    public bool ExpiryAlertActive { get; init; }
    public DateTime? ExpiryLastNotifiedAtUtc { get; init; }
    public DateTime? RenewedAtUtc { get; init; }

    public List<ContractMilestoneReadModel> Milestones { get; init; } = [];
    public List<ContractAttachmentReadModel> Attachments { get; init; } = [];
}

public sealed class ContractMilestoneReadModel
{
    public Guid MilestoneId { get; init; }
    public string Description { get; init; } = default!;
    public decimal Percentage { get; init; }
    public decimal Amount { get; init; }
    public DateTime ScheduledDate { get; init; }
    public bool IsBilled { get; init; }
    public Guid? InvoiceId { get; init; }
}

public sealed class ContractAttachmentReadModel
{
    public Guid AttachmentId { get; init; }
    public string FileName { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public string StorageKey { get; init; } = default!;
    public long Size { get; init; }
    public DateTime UploadedAt { get; init; }
}
