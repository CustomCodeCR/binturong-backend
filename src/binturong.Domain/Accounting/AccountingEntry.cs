using SharedKernel;

namespace Domain.Accounting;

public sealed class AccountingEntry : Entity
{
    public Guid Id { get; set; }

    // Income | Expense
    public string EntryType { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public DateTime EntryDateUtc { get; set; }

    public Guid? ClientId { get; set; }
    public Guid? SupplierId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;
    public string ReceiptFileS3Key { get; set; } = string.Empty;

    public bool IsReconciled { get; set; }
    public Guid? ReconciliationId { get; set; }

    public void RaiseCreated() =>
        Raise(
            new AccountingEntryCreatedDomainEvent(
                Id,
                EntryType,
                Amount,
                Detail,
                Category,
                EntryDateUtc,
                ClientId,
                SupplierId,
                string.IsNullOrWhiteSpace(InvoiceNumber) ? null : InvoiceNumber,
                string.IsNullOrWhiteSpace(ReceiptFileS3Key) ? null : ReceiptFileS3Key,
                IsReconciled
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new AccountingEntryUpdatedDomainEvent(
                Id,
                Amount,
                Detail,
                Category,
                EntryDateUtc,
                ClientId,
                SupplierId,
                string.IsNullOrWhiteSpace(InvoiceNumber) ? null : InvoiceNumber,
                string.IsNullOrWhiteSpace(ReceiptFileS3Key) ? null : ReceiptFileS3Key,
                IsReconciled,
                DateTime.UtcNow
            )
        );

    public void RaiseDeleted() => Raise(new AccountingEntryDeletedDomainEvent(Id));

    public Result Validate()
    {
        if (string.IsNullOrWhiteSpace(EntryType))
            return Result.Failure(AccountingErrors.EntryTypeRequired);

        if (
            !string.Equals(EntryType, "Income", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(EntryType, "Expense", StringComparison.OrdinalIgnoreCase)
        )
        {
            return Result.Failure(
                Error.Validation(
                    "Accounting.InvalidEntryType",
                    "EntryType must be Income or Expense."
                )
            );
        }

        if (Amount <= 0)
            return Result.Failure(AccountingErrors.AmountInvalid);

        if (string.IsNullOrWhiteSpace(Detail))
            return Result.Failure(AccountingErrors.DetailRequired);

        if (string.IsNullOrWhiteSpace(Category))
            return Result.Failure(AccountingErrors.CategoryRequired);

        if (
            string.Equals(EntryType, "Income", StringComparison.OrdinalIgnoreCase)
            && ClientId == Guid.Empty
        )
            return Result.Failure(AccountingErrors.ClientRequired);

        if (
            string.Equals(EntryType, "Expense", StringComparison.OrdinalIgnoreCase)
            && SupplierId == Guid.Empty
        )
            return Result.Failure(AccountingErrors.SupplierRequired);

        return Result.Success();
    }

    public Result Reconcile(Guid reconciliationId, decimal matchedAmount, DateTime reconciledAtUtc)
    {
        if (IsReconciled)
            return Result.Failure(AccountingErrors.AlreadyReconciled);

        if (matchedAmount <= 0 || matchedAmount > Amount)
            return Result.Failure(AccountingErrors.AmountInvalid);

        IsReconciled = true;
        ReconciliationId = reconciliationId;

        Raise(
            new AccountingEntryReconciledDomainEvent(
                Id,
                reconciliationId,
                matchedAmount,
                reconciledAtUtc
            )
        );
        return Result.Success();
    }

    public Result Unreconcile(DateTime nowUtc)
    {
        if (!IsReconciled)
            return Result.Failure(AccountingErrors.EntryNotReconcilable);

        IsReconciled = false;
        ReconciliationId = null;

        Raise(new AccountingEntryUnreconciledDomainEvent(Id, nowUtc));
        return Result.Success();
    }
}
