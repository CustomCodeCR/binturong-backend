using Application.Abstractions.Data;
using Application.Abstractions.EInvoicing;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Invoices.Emit;

internal sealed class EmitInvoiceCommandHandler
    : ICommandHandler<EmitInvoiceCommand, EmitInvoiceResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IElectronicInvoicingService _einvoicing;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public EmitInvoiceCommandHandler(
        IApplicationDbContext db,
        IElectronicInvoicingService einvoicing,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _einvoicing = einvoicing;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<EmitInvoiceResponse>> Handle(
        EmitInvoiceCommand cmd,
        CancellationToken ct
    )
    {
        var inv = await _db
            .Invoices.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);
        if (inv is null)
            return Result.Failure<EmitInvoiceResponse>(
                Error.NotFound("Invoices.NotFound", $"Invoice '{cmd.InvoiceId}' not found.")
            );

        // HU-FAC-01 validation scenario (#2)
        if (inv.ClientId == Guid.Empty)
            return Result.Failure<EmitInvoiceResponse>(
                Error.Validation("Invoices.ClientRequired", "ClientId is required.")
            );

        if (string.IsNullOrWhiteSpace(inv.Currency))
            return Result.Failure<EmitInvoiceResponse>(
                Error.Validation("Invoices.CurrencyRequired", "Currency is required.")
            );

        if (inv.Details.Count == 0)
            return Result.Failure<EmitInvoiceResponse>(
                Error.Validation("Invoices.LinesRequired", "Invoice must have lines.")
            );

        inv.RaiseEmissionRequested("Normal", DateTime.UtcNow);

        var r = await _einvoicing.EmitInvoiceAsync(cmd.InvoiceId, ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Invoices",
            "Invoice",
            cmd.InvoiceId,
            "INVOICE_EMIT",
            string.Empty,
            $"mode={r.Mode}; taxStatus={r.TaxStatus}; taxKey={r.TaxKey}; consecutive={r.Consecutive}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        if (!r.IsSuccess)
            return Result.Failure<EmitInvoiceResponse>(
                Error.Validation("Invoices.EmitFailed", r.Message ?? "Emission failed.")
            );

        return Result.Success(
            new EmitInvoiceResponse(
                r.Mode,
                r.TaxStatus,
                r.TaxKey,
                r.Consecutive,
                r.PdfKey,
                r.XmlKey,
                r.Message
            )
        );
    }
}
