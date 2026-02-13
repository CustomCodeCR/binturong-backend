using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.DebitNotes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.DebitNotes.Delete;

internal sealed class DeleteDebitNoteCommandHandler : ICommandHandler<DeleteDebitNoteCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteDebitNoteCommandHandler(
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

    public async Task<Result> Handle(DeleteDebitNoteCommand cmd, CancellationToken ct)
    {
        var dn = await _db.DebitNotes.FirstOrDefaultAsync(x => x.Id == cmd.DebitNoteId, ct);
        if (dn is null)
            return Result.Failure(DebitNoteErrors.NotFound(cmd.DebitNoteId));

        dn.RaiseDeleted();

        _db.DebitNotes.Remove(dn);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "DebitNotes",
            "DebitNote",
            cmd.DebitNoteId,
            "DEBIT_NOTE_DELETED",
            string.Empty,
            "ok",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
