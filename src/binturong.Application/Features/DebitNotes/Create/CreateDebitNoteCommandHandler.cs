using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.DebitNotes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.DebitNotes.Create;

internal sealed class CreateDebitNoteCommandHandler : ICommandHandler<CreateDebitNoteCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateDebitNoteCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateDebitNoteCommand cmd, CancellationToken ct)
    {
        if (cmd.InvoiceId == Guid.Empty)
            return Result.Failure<Guid>(DebitNoteErrors.InvoiceRequired);

        if (string.IsNullOrWhiteSpace(cmd.Reason))
            return Result.Failure<Guid>(DebitNoteErrors.ReasonRequired);

        if (cmd.TotalAmount <= 0)
            return Result.Failure<Guid>(DebitNoteErrors.TotalAmountInvalid);

        var invoice = await _db.Invoices.FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);
        if (invoice is null)
            return Result.Failure<Guid>(DebitNoteErrors.InvoiceNotFound(cmd.InvoiceId));

        if (invoice.TaxStatus != "Emitted")
            return Result.Failure<Guid>(DebitNoteErrors.InvoiceNotEmitted);

        var dn = new DebitNote
        {
            Id = Guid.NewGuid(),
            InvoiceId = cmd.InvoiceId,
            IssueDate = cmd.IssueDate,
            Reason = cmd.Reason.Trim(),
            TotalAmount = cmd.TotalAmount,
            TaxStatus = "Draft",
        };

        dn.RaiseCreated();

        _db.DebitNotes.Add(dn);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "DebitNotes",
            "DebitNote",
            dn.Id,
            "DEBIT_NOTE_CREATED",
            string.Empty,
            $"invoiceId={dn.InvoiceId}; total={dn.TotalAmount}; reason={dn.Reason}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(dn.Id);
    }
}
