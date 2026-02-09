using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.SupplierQuotes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.SupplierQuotes.Respond;

internal sealed class RespondSupplierQuoteCommandHandler
    : ICommandHandler<RespondSupplierQuoteCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RespondSupplierQuoteCommandHandler(
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

    public async Task<Result> Handle(RespondSupplierQuoteCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var quote = await _db
            .SupplierQuotes.Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == cmd.SupplierQuoteId, ct);

        if (quote is null)
            return Result.Failure(SupplierQuoteErrors.NotFound(cmd.SupplierQuoteId));

        var result = quote.RegisterResponse(cmd.RespondedAtUtc, cmd.SupplierMessage, cmd.Lines);
        if (result.IsFailure)
            return result;

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Suppliers",
            "SupplierQuote",
            quote.Id,
            "SUPPLIER_QUOTE_RESPONDED",
            string.Empty,
            $"supplierQuoteId={quote.Id}; respondedAt={cmd.RespondedAtUtc:o}; lines={cmd.Lines.Count}; status={quote.Status}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
