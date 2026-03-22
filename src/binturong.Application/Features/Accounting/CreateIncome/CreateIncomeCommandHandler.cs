using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Accounting.CreateIncome;

internal sealed class CreateIncomeCommandHandler : ICommandHandler<CreateIncomeCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateIncomeCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateIncomeCommand cmd, CancellationToken ct)
    {
        if (cmd.ClientId == Guid.Empty)
            return Result.Failure<Guid>(AccountingErrors.ClientRequired);

        if (string.IsNullOrWhiteSpace(cmd.InvoiceNumber))
            return Result.Failure<Guid>(AccountingErrors.InvoiceNumberRequired);

        var duplicated = await _db.AccountingEntries.AnyAsync(
            x =>
                x.EntryType == "Income"
                && x.EntryDateUtc.Date == cmd.EntryDateUtc.Date
                && x.InvoiceNumber == cmd.InvoiceNumber,
            ct
        );

        if (duplicated)
            return Result.Failure<Guid>(AccountingErrors.DuplicateIncome);

        var entry = new AccountingEntry
        {
            Id = Guid.NewGuid(),
            EntryType = "Income",
            Amount = cmd.Amount,
            Detail = cmd.Detail?.Trim() ?? string.Empty,
            Category = cmd.Category?.Trim() ?? string.Empty,
            EntryDateUtc = EnsureUtc(cmd.EntryDateUtc),
            ClientId = cmd.ClientId,
            SupplierId = null,
            InvoiceNumber = cmd.InvoiceNumber?.Trim() ?? string.Empty,
            ReceiptFileS3Key = string.Empty,
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
