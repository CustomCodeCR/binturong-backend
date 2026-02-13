using Application.Abstractions.Data;
using Application.Abstractions.EInvoicing;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.DebitNotes.Emit;

internal sealed class EmitDebitNoteCommandHandler
    : ICommandHandler<EmitDebitNoteCommand, EmitDebitNoteResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IElectronicInvoicingService _einvoicing;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public EmitDebitNoteCommandHandler(
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

    public async Task<Result<EmitDebitNoteResponse>> Handle(
        EmitDebitNoteCommand cmd,
        CancellationToken ct
    )
    {
        if (cmd.DebitNoteId == Guid.Empty)
            return Result.Failure<EmitDebitNoteResponse>(
                Error.Validation("DebitNotes.IdRequired", "DebitNoteId is required.")
            );

        var dn = await _db.DebitNotes.FirstOrDefaultAsync(x => x.Id == cmd.DebitNoteId, ct);
        if (dn is null)
            return Result.Failure<EmitDebitNoteResponse>(
                Error.NotFound("DebitNotes.NotFound", $"DebitNote '{cmd.DebitNoteId}' not found.")
            );

        var r = await _einvoicing.EmitDebitNoteAsync(cmd.DebitNoteId, ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "DebitNotes",
            "DebitNote",
            cmd.DebitNoteId,
            "DEBIT_NOTE_EMIT",
            string.Empty,
            $"mode={r.Mode}; taxStatus={r.TaxStatus}; taxKey={r.TaxKey}; consecutive={r.Consecutive}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        if (!r.IsSuccess)
            return Result.Failure<EmitDebitNoteResponse>(
                Error.Validation("DebitNotes.EmitFailed", r.Message ?? "Emission failed.")
            );

        return Result.Success(
            new EmitDebitNoteResponse(
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
