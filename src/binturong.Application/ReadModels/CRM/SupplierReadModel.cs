using Application.ReadModels.Common;

namespace Application.ReadModels.CRM;

public sealed class SupplierReadModel
{
    public string Id { get; init; } = default!; // "supplier:{SupplierId}"
    public Guid SupplierId { get; init; }

    public string IdentificationType { get; init; } = default!;
    public string Identification { get; init; } = default!;

    public string LegalName { get; init; } = default!;
    public string TradeName { get; init; } = default!;

    public string Email { get; init; } = default!;
    public string Phone { get; init; } = default!;

    public string? PaymentTerms { get; init; }
    public string? MainCurrency { get; init; }

    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<ContactReadModel> Contacts { get; init; } = [];
    public IReadOnlyList<AttachmentReadModel> Attachments { get; init; } = [];

    // Payables KPI
    public decimal PendingPayables { get; init; }
}
