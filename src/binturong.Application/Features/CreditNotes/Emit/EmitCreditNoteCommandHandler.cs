using Application.Abstractions.Data;
using Application.Abstractions.EInvoicing;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.CreditNotes.Emit;

internal sealed class EmitCreditNoteCommandHandler
    : ICommandHandler<EmitCreditNoteCommand, EmitCreditNoteResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IElectronicInvoicingService _einvoicing;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public EmitCreditNoteCommandHandler(
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

    public async Task<Result<EmitCreditNoteResponse>> Handle(
        EmitCreditNoteCommand cmd,
        CancellationToken ct
    )
    {
        if (cmd.CreditNoteId == Guid.Empty)
            return Result.Failure<EmitCreditNoteResponse>(
                Error.Validation("CreditNotes.IdRequired", "CreditNoteId is required.")
            );

        var cn = await _db.CreditNotes.FirstOrDefaultAsync(x => x.Id == cmd.CreditNoteId, ct);
        if (cn is null)
            return Result.Failure<EmitCreditNoteResponse>(
                Error.NotFound(
                    "CreditNotes.NotFound",
                    $"CreditNote '{cmd.CreditNoteId}' not found."
                )
            );

        var r = await _einvoicing.EmitCreditNoteAsync(cmd.CreditNoteId, ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "CreditNotes",
            "CreditNote",
            cmd.CreditNoteId,
            "CREDIT_NOTE_EMIT",
            string.Empty,
            $"mode={r.Mode}; taxStatus={r.TaxStatus}; taxKey={r.TaxKey}; consecutive={r.Consecutive}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        if (!r.IsSuccess)
            return Result.Failure<EmitCreditNoteResponse>(
                Error.Validation("CreditNotes.EmitFailed", r.Message ?? "Emission failed.")
            );

        return Result.Success(
            new EmitCreditNoteResponse(
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
