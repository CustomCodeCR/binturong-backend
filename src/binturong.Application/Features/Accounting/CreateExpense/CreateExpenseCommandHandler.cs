using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Accounting;
using SharedKernel;

namespace Application.Features.Accounting.CreateExpense;

internal sealed class CreateExpenseCommandHandler : ICommandHandler<CreateExpenseCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateExpenseCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateExpenseCommand cmd, CancellationToken ct)
    {
        if (cmd.SupplierId == Guid.Empty)
            return Result.Failure<Guid>(AccountingErrors.SupplierRequired);

        var entry = new AccountingEntry
        {
            Id = Guid.NewGuid(),
            EntryType = "Expense",
            Amount = cmd.Amount,
            Detail = cmd.Detail?.Trim() ?? string.Empty,
            Category = cmd.Category?.Trim() ?? string.Empty,
            EntryDateUtc = EnsureUtc(cmd.EntryDateUtc),
            ClientId = null,
            SupplierId = cmd.SupplierId,
            InvoiceNumber = string.Empty,
            ReceiptFileS3Key = cmd.ReceiptFileS3Key?.Trim() ?? string.Empty,
            IsReconciled = false,
        };

        var validation = entry.Validate();
        if (validation.IsFailure)
            return Result.Failure<Guid>(validation.Error);

        entry.RaiseCreated();

        _db.AccountingEntries.Add(entry);
        await _db.SaveChangesAsync(ct);

        return Result.Success(entry.Id);
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
    }
}
