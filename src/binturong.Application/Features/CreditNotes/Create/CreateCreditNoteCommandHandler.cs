using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.CreditNotes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.CreditNotes.Create;

internal sealed class CreateCreditNoteCommandHandler
    : ICommandHandler<CreateCreditNoteCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateCreditNoteCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateCreditNoteCommand cmd, CancellationToken ct)
    {
        if (cmd.InvoiceId == Guid.Empty)
            return Result.Failure<Guid>(CreditNoteErrors.InvoiceRequired);

        if (string.IsNullOrWhiteSpace(cmd.Reason))
            return Result.Failure<Guid>(CreditNoteErrors.ReasonRequired);

        if (cmd.TotalAmount <= 0)
            return Result.Failure<Guid>(CreditNoteErrors.TotalAmountInvalid);

        var authorizedReasons = new[] { "Return", "PriceCorrection", "ServiceCanceled" };
        if (!authorizedReasons.Contains(cmd.Reason.Trim()))
            return Result.Failure<Guid>(CreditNoteErrors.ReasonNotAuthorized);

        var invoice = await _db.Invoices.FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);
        if (invoice is null)
            return Result.Failure<Guid>(CreditNoteErrors.InvoiceNotFound(cmd.InvoiceId));

        if (invoice.TaxStatus != "Emitted")
            return Result.Failure<Guid>(CreditNoteErrors.InvoiceNotEmitted);

        var cn = new CreditNote
        {
            Id = Guid.NewGuid(),
            InvoiceId = cmd.InvoiceId,
            IssueDate = cmd.IssueDate,
            Reason = cmd.Reason.Trim(),
            TotalAmount = cmd.TotalAmount,
            TaxStatus = "Draft",
        };

        cn.RaiseCreated();

        _db.CreditNotes.Add(cn);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "CreditNotes",
            "CreditNote",
            cn.Id,
            "CREDIT_NOTE_CREATED",
            string.Empty,
            $"invoiceId={cn.InvoiceId}; total={cn.TotalAmount}; reason={cn.Reason}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(cn.Id);
    }
}
