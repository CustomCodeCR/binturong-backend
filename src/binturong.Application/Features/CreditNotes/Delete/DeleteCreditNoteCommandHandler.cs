using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.CreditNotes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.CreditNotes.Delete;

internal sealed class DeleteCreditNoteCommandHandler : ICommandHandler<DeleteCreditNoteCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteCreditNoteCommandHandler(
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

    public async Task<Result> Handle(DeleteCreditNoteCommand cmd, CancellationToken ct)
    {
        var cn = await _db.CreditNotes.FirstOrDefaultAsync(x => x.Id == cmd.CreditNoteId, ct);
        if (cn is null)
            return Result.Failure(CreditNoteErrors.NotFound(cmd.CreditNoteId));

        cn.RaiseDeleted();

        _db.CreditNotes.Remove(cn);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "CreditNotes",
            "CreditNote",
            cmd.CreditNoteId,
            "CREDIT_NOTE_DELETED",
            string.Empty,
            "ok",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
