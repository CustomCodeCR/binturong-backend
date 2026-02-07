using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Quotes.Reject;

internal sealed class RejectQuoteCommandHandler : ICommandHandler<RejectQuoteCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RejectQuoteCommandHandler(
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

    public async Task<Result> Handle(RejectQuoteCommand command, CancellationToken ct)
    {
        var quote = await _db.Quotes.FirstOrDefaultAsync(x => x.Id == command.QuoteId, ct);
        if (quote is null)
            return Result.Failure(
                Error.NotFound("Quotes.NotFound", $"Quote '{command.QuoteId}' not found")
            );

        var reason = command.Reason?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(Error.Validation("Quotes.ReasonRequired", "Reason is required"));

        quote.Reject(reason);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Sales",
            "Quote",
            quote.Id,
            "QUOTE_REJECTED",
            string.Empty,
            $"quoteId={quote.Id}; reason={reason}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
