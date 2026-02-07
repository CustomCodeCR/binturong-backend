using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Quotes.Send;

internal sealed class SendQuoteCommandHandler : ICommandHandler<SendQuoteCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public SendQuoteCommandHandler(
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

    public async Task<Result> Handle(SendQuoteCommand command, CancellationToken ct)
    {
        var quote = await _db.Quotes.FirstOrDefaultAsync(x => x.Id == command.QuoteId, ct);
        if (quote is null)
            return Result.Failure(
                Error.NotFound("Quotes.NotFound", $"Quote '{command.QuoteId}' not found")
            );

        quote.Send();
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Sales",
            "Quote",
            quote.Id,
            "QUOTE_SENT",
            string.Empty,
            $"quoteId={quote.Id}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
