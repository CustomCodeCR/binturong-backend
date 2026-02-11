using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Storage;
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
    private readonly IObjectStorage _storage;
    private readonly IObjectStorageKeyBuilder _keys;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    private const long MaxBytes = 20 * 1024 * 1024; // 20MB

    public UploadClientAttachmentCommandHandler(
        IApplicationDbContext db,
        IObjectStorage storage,
        IObjectStorageKeyBuilder keys,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _storage = storage;
        _keys = keys;
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

        if (command.Content is null || command.SizeBytes <= 0)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientAttachment",
                null,
                "CLIENT_ATTACHMENT_UPLOAD_FAILED",
                string.Empty,
                $"reason=file_missing; clientId={command.ClientId}; fileName={command.FileName}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(ClientAttachmentErrors.FileNameIsRequired);
        }

        if (command.SizeBytes > MaxBytes)
            return Result.Failure<Guid>(
                Error.Validation("Clients.Attachments.TooLarge", "File is too large.")
            );

        var key = _keys.Build("clients", command.ClientId, command.FileName);

        if (command.Content.CanSeek)
            command.Content.Position = 0;

        await _storage.PutAsync(
            key,
            command.Content,
            string.IsNullOrWhiteSpace(command.ContentType)
                ? "application/octet-stream"
                : command.ContentType.Trim(),
            ct
        );

        var now = DateTime.UtcNow;

        var attachment = new ClientAttachment
        {
            Id = Guid.NewGuid(),
            ClientId = command.ClientId,
            FileName = command.FileName.Trim(),
            FileS3Key = key, // keep your field name
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
            $"clientId={command.ClientId}; attachmentId={attachment.Id}; fileName={attachment.FileName}; documentType={attachment.DocumentType}; key={attachment.FileS3Key}",
            ip,
            ua,
            ct
        );

        return Result.Success(attachment.Id);
    }
}
