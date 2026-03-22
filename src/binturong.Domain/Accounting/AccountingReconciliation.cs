using SharedKernel;

namespace Domain.Accounting;

public sealed class AccountingReconciliation : Entity
{
    public Guid Id { get; set; }
    public Guid AccountingEntryId { get; set; }

    // Invoice | PurchaseOrder | Manual
    public string SourceType { get; set; } = string.Empty;
    public Guid? SourceId { get; set; }

    public decimal MatchedAmount { get; set; }
    public DateTime ReconciledAtUtc { get; set; }

    public void RaiseCreated() =>
        Raise(
            new AccountingReconciliationCreatedDomainEvent(
                Id,
                AccountingEntryId,
                SourceType,
                SourceId,
                MatchedAmount,
                ReconciledAtUtc
            )
        );

    public void RaiseDeleted() => Raise(new AccountingReconciliationDeletedDomainEvent(Id));
}
