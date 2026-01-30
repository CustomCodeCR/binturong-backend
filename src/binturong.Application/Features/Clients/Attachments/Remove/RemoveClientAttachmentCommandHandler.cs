using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ClientAttachments;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Attachments.Remove;

internal sealed class RemoveClientAttachmentCommandHandler
    : ICommandHandler<RemoveClientAttachmentCommand>
{
    private readonly IApplicationDbContext _db;

    public RemoveClientAttachmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(RemoveClientAttachmentCommand command, CancellationToken ct)
    {
        var clientExists = await _db.Clients.AnyAsync(x => x.Id == command.ClientId, ct);
        if (!clientExists)
            return Result.Failure(ClientErrors.NotFound(command.ClientId));

        var attachment = await _db.ClientAttachments.FirstOrDefaultAsync(
            x => x.Id == command.AttachmentId && x.ClientId == command.ClientId,
            ct
        );

        if (attachment is null)
            return Result.Failure(ClientAttachmentErrors.NotFound(command.AttachmentId));

        attachment.RaiseDeleted();

        _db.ClientAttachments.Remove(attachment);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
