using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.SupplierQuotes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.SupplierQuotes.Create;

internal sealed class CreateSupplierQuoteCommandHandler
    : ICommandHandler<CreateSupplierQuoteCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateSupplierQuoteCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateSupplierQuoteCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var code = cmd.Code.Trim();
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Guid>(SupplierQuoteErrors.CodeRequired);

        if (cmd.SupplierId == Guid.Empty)
            return Result.Failure<Guid>(SupplierQuoteErrors.SupplierRequired);

        if (cmd.Lines is null || cmd.Lines.Count == 0)
            return Result.Failure<Guid>(SupplierQuoteErrors.NoLines);

        if (cmd.Lines.Any(x => x.Quantity <= 0))
            return Result.Failure<Guid>(SupplierQuoteErrors.LineQuantityInvalid);

        var exists = await _db.SupplierQuotes.AnyAsync(x => x.Code.ToLower() == code.ToLower(), ct);
        if (exists)
            return Result.Failure<Guid>(
                Error.Validation("SupplierQuotes.CodeNotUnique", "Code must be unique.")
            );

        var quote = new SupplierQuote
        {
            Id = Guid.NewGuid(),
            Code = code,
            SupplierId = cmd.SupplierId,
            BranchId = cmd.BranchId,
            RequestedAtUtc = cmd.RequestedAtUtc,
            Status = "Sent", // HU-COM-04 Scenario 1
            Notes = cmd.Notes?.Trim() ?? string.Empty,
        };

        foreach (var l in cmd.Lines)
        {
            var r = quote.AddLine(l.ProductId, l.Quantity);
            if (r.IsFailure)
                return Result.Failure<Guid>(r.Error);
        }

        quote.RaiseSent();

        _db.SupplierQuotes.Add(quote);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Suppliers",
            "SupplierQuote",
            quote.Id,
            "SUPPLIER_QUOTE_SENT",
            string.Empty,
            $"supplierQuoteId={quote.Id}; code={quote.Code}; supplierId={quote.SupplierId}; lines={cmd.Lines.Count}; status={quote.Status}",
            ip,
            ua,
            ct
        );

        return Result.Success(quote.Id);
    }
}
