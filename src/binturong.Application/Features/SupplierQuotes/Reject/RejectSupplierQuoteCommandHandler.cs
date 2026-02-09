using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.SupplierQuotes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.SupplierQuotes.Reject;

internal sealed class RejectSupplierQuoteCommandHandler
    : ICommandHandler<RejectSupplierQuoteCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RejectSupplierQuoteCommandHandler(
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

    public async Task<Result> Handle(RejectSupplierQuoteCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var quote = await _db.SupplierQuotes.FirstOrDefaultAsync(
            x => x.Id == cmd.SupplierQuoteId,
            ct
        );
        if (quote is null)
            return Result.Failure(SupplierQuoteErrors.NotFound(cmd.SupplierQuoteId));

        var result = quote.Reject(cmd.Reason, cmd.RejectedAtUtc);
        if (result.IsFailure)
            return result;

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Suppliers",
            "SupplierQuote",
            quote.Id,
            "SUPPLIER_QUOTE_REJECTED",
            string.Empty,
            $"supplierQuoteId={quote.Id}; rejectedAt={cmd.RejectedAtUtc:o}; reason={cmd.Reason}; status={quote.Status}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
