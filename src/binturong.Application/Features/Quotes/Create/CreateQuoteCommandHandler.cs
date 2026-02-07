using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Quotes.Create;

internal sealed class CreateQuoteCommandHandler : ICommandHandler<CreateQuoteCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateQuoteCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateQuoteCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var code = command.Code.Trim();
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Guid>(
                Error.Validation("Quotes.CodeRequired", "Code is required")
            );

        var codeExists = await _db.Quotes.AnyAsync(x => x.Code.ToLower() == code.ToLower(), ct);
        if (codeExists)
            return Result.Failure<Guid>(
                Error.Validation("Quotes.CodeNotUnique", "Code must be unique")
            );

        if (command.ClientId == Guid.Empty)
            return Result.Failure<Guid>(
                Error.Validation("Quotes.ClientRequired", "ClientId is required")
            );

        var now = DateTime.UtcNow;

        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            Code = code,
            ClientId = command.ClientId,
            BranchId = command.BranchId,
            IssueDate = command.IssueDate,
            ValidUntil = command.ValidUntil,
            Status = "Draft",
            Currency = command.Currency.Trim(),
            ExchangeRate = command.ExchangeRate,
            Notes = command.Notes?.Trim() ?? string.Empty,
            CreatedAt = now,
            UpdatedAt = now,

            // Totals iniciales (sin líneas)
            Subtotal = 0,
            Taxes = 0,
            Discounts = 0,
            Total = 0,
            AcceptedByClient = false,
            AcceptanceDate = null,
            Version = 1,
        };

        quote.RaiseCreated();

        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Sales",
            "Quote",
            quote.Id,
            "QUOTE_CREATED",
            string.Empty,
            $"quoteId={quote.Id}; code={quote.Code}; clientId={quote.ClientId}; branchId={quote.BranchId}; currency={quote.Currency}",
            ip,
            ua,
            ct
        );

        return Result.Success(quote.Id);
    }
}
