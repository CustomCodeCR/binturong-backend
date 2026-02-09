using SharedKernel;

namespace Domain.AccountsPayable;

public sealed class AccountPayable : Entity
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string SupplierInvoiceId { get; set; } = string.Empty;
    public DateOnly DocumentDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PendingBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public Domain.Suppliers.Supplier? Supplier { get; set; }
    public Domain.PurchaseOrders.PurchaseOrder? PurchaseOrder { get; set; }

    public void RaiseCreated() =>
        Raise(
            new AccountPayableCreatedDomainEvent(
                Id,
                SupplierId,
                PurchaseOrderId,
                SupplierInvoiceId,
                DocumentDate,
                DueDate,
                TotalAmount,
                PendingBalance,
                Currency,
                Status
            )
        );

    public Result RegisterPayment(decimal amount, DateTime paidAtUtc, string? notes)
    {
        if (amount <= 0)
            return Result.Failure(AccountPayableErrors.PaymentAmountInvalid);

        if (PendingBalance <= 0)
            return Result.Failure(AccountPayableErrors.AlreadySettled);

        if (amount > PendingBalance)
            return Result.Failure(AccountPayableErrors.PaymentExceedsBalance);

        var before = PendingBalance;
        PendingBalance -= amount;

        Status = PendingBalance == 0 ? "Paid" : "Pending";

        Raise(
            new AccountPayablePaymentRegisteredDomainEvent(
                Id,
                amount,
                before,
                PendingBalance,
                Status,
                paidAtUtc,
                string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
            )
        );

        return Result.Success();
    }
}
