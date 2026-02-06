using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ClientAttachments;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Attachments.Upload;

internal sealed class UploadClientAttachmentCommandHandler
    : ICommandHandler<UploadClientAttachmentCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UploadClientAttachmentCommandHandler(
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

    public async Task<Result<Guid>> Handle(
        UploadClientAttachmentCommand command,
        CancellationToken ct
    )
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
                null,
                "CLIENT_ATTACHMENT_UPLOAD_FAILED",
                string.Empty,
                $"reason=client_not_found; clientId={command.ClientId}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(ClientErrors.NotFound(command.ClientId));
        }

        if (string.IsNullOrWhiteSpace(command.FileName))
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientAttachment",
                null,
                "CLIENT_ATTACHMENT_UPLOAD_FAILED",
                string.Empty,
                $"reason=file_name_required; clientId={command.ClientId}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(ClientAttachmentErrors.FileNameIsRequired);
        }

        if (string.IsNullOrWhiteSpace(command.FileS3Key))
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientAttachment",
                null,
                "CLIENT_ATTACHMENT_UPLOAD_FAILED",
                string.Empty,
                $"reason=file_s3_key_required; clientId={command.ClientId}; fileName={command.FileName}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(ClientAttachmentErrors.FileS3KeyIsRequired);
        }

        if (string.IsNullOrWhiteSpace(command.DocumentType))
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientAttachment",
                null,
                "CLIENT_ATTACHMENT_UPLOAD_FAILED",
                string.Empty,
                $"reason=document_type_required; clientId={command.ClientId}; fileName={command.FileName}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(ClientAttachmentErrors.DocumentTypeIsRequired);
        }

        var now = DateTime.UtcNow;

        var attachment = new ClientAttachment
        {
            Id = Guid.NewGuid(),
            ClientId = command.ClientId,
            FileName = command.FileName.Trim(),
            FileS3Key = command.FileS3Key.Trim(),
            DocumentType = command.DocumentType.Trim(),
            UploadedAt = now,
            UpdatedAt = now,
        };

        attachment.RaiseUploaded();

        _db.ClientAttachments.Add(attachment);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Clients",
            "ClientAttachment",
            attachment.Id,
            "CLIENT_ATTACHMENT_UPLOADED",
            string.Empty,
            $"clientId={command.ClientId}; attachmentId={attachment.Id}; fileName={attachment.FileName}; documentType={attachment.DocumentType}; s3Key={attachment.FileS3Key}",
            ip,
            ua,
            ct
        );

        return Result.Success(attachment.Id);
    }
}
