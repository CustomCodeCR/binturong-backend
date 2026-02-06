using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ClientAttachments;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Attachments.Remove;

internal sealed class RemoveClientAttachmentCommandHandler
    : ICommandHandler<RemoveClientAttachmentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RemoveClientAttachmentCommandHandler(
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

    public async Task<Result> Handle(RemoveClientAttachmentCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var clientExists = await _db.Clients.AnyAsync(x => x.Id == command.ClientId, ct);
        if (!clientExists)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientAttachment",
                command.AttachmentId,
                "CLIENT_ATTACHMENT_REMOVE_FAILED",
                string.Empty,
                $"reason=client_not_found; clientId={command.ClientId}; attachmentId={command.AttachmentId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientErrors.NotFound(command.ClientId));
        }

        var attachment = await _db.ClientAttachments.FirstOrDefaultAsync(
            x => x.Id == command.AttachmentId && x.ClientId == command.ClientId,
            ct
        );

        if (attachment is null)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientAttachment",
                command.AttachmentId,
                "CLIENT_ATTACHMENT_REMOVE_FAILED",
                string.Empty,
                $"reason=not_found; clientId={command.ClientId}; attachmentId={command.AttachmentId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientAttachmentErrors.NotFound(command.AttachmentId));
        }

        var before =
            $"clientId={attachment.ClientId}; fileName={attachment.FileName}; documentType={attachment.DocumentType}; s3Key={attachment.FileS3Key}";

        attachment.RaiseDeleted();

        _db.ClientAttachments.Remove(attachment);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Clients",
            "ClientAttachment",
            attachment.Id,
            "CLIENT_ATTACHMENT_REMOVED",
            before,
            $"clientId={command.ClientId}; attachmentId={attachment.Id}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
