using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Contracts.ConvertFromQuote;

internal sealed class ConvertQuoteToContractCommandHandler
    : ICommandHandler<ConvertQuoteToContractCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public ConvertQuoteToContractCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(ConvertQuoteToContractCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (cmd.QuoteId == Guid.Empty)
            return Result.Failure<Guid>(ContractErrors.QuoteRequired);

        if (cmd.ResponsibleUserId == Guid.Empty)
            return Result.Failure<Guid>(ContractErrors.ResponsibleUserRequired);

        if (cmd.EndDate is not null && cmd.EndDate.Value.DayNumber < cmd.StartDate.DayNumber)
            return Result.Failure<Guid>(
                ContractErrors.InvalidValidity(cmd.StartDate, cmd.EndDate.Value)
            );

        var quote = await _db.Quotes.FirstOrDefaultAsync(x => x.Id == cmd.QuoteId, ct);
        if (quote is null)
            return Result.Failure<Guid>(ContractErrors.QuoteNotFound(cmd.QuoteId));

        if (!string.Equals(quote.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
        {
            await _bus.AuditAsync(
                userId,
                "Contracts",
                "Contract",
                null,
                "CONTRACT_CONVERT_FROM_QUOTE_FAILED",
                string.Empty,
                $"reason=quote_not_accepted; quoteId={cmd.QuoteId}; status={quote.Status}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(ContractErrors.QuoteNotAccepted);
        }

        var nowUtc = DateTime.UtcNow;

        var contract = new Domain.Contracts.Contract
        {
            Id = Guid.NewGuid(),
            Code = await NextCodeAsync(ct),
            ClientId = quote.ClientId,
            QuoteId = quote.Id,
            SalesOrderId = null,
            StartDate = cmd.StartDate,
            EndDate = cmd.EndDate,
            Status = "Active",
            Description = cmd.Description?.Trim() ?? string.Empty,
            Notes = cmd.Notes?.Trim() ?? string.Empty,
            AutoRenewEnabled = cmd.AutoRenewEnabled,
            AutoRenewEveryDays = Math.Max(1, cmd.AutoRenewEveryDays),
            ExpiryNoticeDays = Math.Max(0, cmd.ExpiryNoticeDays),
            ResponsibleUserId = cmd.ResponsibleUserId,
        };

        contract.RaiseCreated();
        contract.Raise(
            new ContractCreatedFromQuoteDomainEvent(
                contract.Id,
                quote.Id,
                contract.StartDate,
                contract.EndDate,
                cmd.ResponsibleUserId,
                nowUtc
            )
        );

        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Contracts",
            "Contract",
            contract.Id,
            "CONTRACT_CREATED_FROM_QUOTE",
            $"quoteId={quote.Id}; quoteStatus={quote.Status}",
            $"contractId={contract.Id}; code={contract.Code}; clientId={contract.ClientId}; startDate={contract.StartDate}; endDate={contract.EndDate}; autoRenew={contract.AutoRenewEnabled}; everyDays={contract.AutoRenewEveryDays}; noticeDays={contract.ExpiryNoticeDays}",
            ip,
            ua,
            ct
        );

        return Result.Success(contract.Id);
    }

    private async Task<string> NextCodeAsync(CancellationToken ct)
    {
        var count = await _db.Contracts.CountAsync(ct);
        return $"CT-{(count + 1):000000}";
    }
}
