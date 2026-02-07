using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Quotes.Accept;

internal sealed class AcceptQuoteCommandHandler : ICommandHandler<AcceptQuoteCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public AcceptQuoteCommandHandler(
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

    public async Task<Result> Handle(AcceptQuoteCommand command, CancellationToken ct)
    {
        var quote = await _db.Quotes.FirstOrDefaultAsync(x => x.Id == command.QuoteId, ct);
        if (quote is null)
            return Result.Failure(
                Error.NotFound("Quotes.NotFound", $"Quote '{command.QuoteId}' not found")
            );

        quote.Accept();
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Sales",
            "Quote",
            quote.Id,
            "QUOTE_ACCEPTED",
            string.Empty,
            $"quoteId={quote.Id}; acceptanceDate={quote.AcceptanceDate}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
