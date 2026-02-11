using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Storage;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.SupplierAttachments;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Attachments.Upload;

internal sealed class UploadSupplierAttachmentCommandHandler
    : ICommandHandler<UploadSupplierAttachmentCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly IObjectStorage _storage;
    private readonly IObjectStorageKeyBuilder _keys;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    private const long MaxBytes = 20 * 1024 * 1024; // 20MB

    public UploadSupplierAttachmentCommandHandler(
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
        UploadSupplierAttachmentCommand command,
        CancellationToken ct
    )
    {
        var userId = _currentUser.UserId;
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        var supplierExists = await _db.Suppliers.AnyAsync(x => x.Id == command.SupplierId, ct);
        if (!supplierExists)
        {
            await _bus.AuditAsync(
                userId,
                "Suppliers",
                "SupplierAttachment",
                command.SupplierId,
                "SUPPLIER_ATTACHMENT_UPLOAD_FAILED",
                string.Empty,
                $"reason=supplier_not_found; supplierId={command.SupplierId}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(SupplierErrors.NotFound(command.SupplierId));
        }

        if (string.IsNullOrWhiteSpace(command.FileName))
            return Result.Failure<Guid>(SupplierAttachmentErrors.FileNameIsRequired);

        if (string.IsNullOrWhiteSpace(command.DocumentType))
            return Result.Failure<Guid>(SupplierAttachmentErrors.DocumentTypeIsRequired);

        if (command.Content is null || command.SizeBytes <= 0)
            return Result.Failure<Guid>(
                Error.Validation("Suppliers.Attachments.Missing", "No file was provided.")
            );

        if (command.SizeBytes > MaxBytes)
            return Result.Failure<Guid>(
                Error.Validation("Suppliers.Attachments.TooLarge", "File is too large.")
            );

        var key = _keys.Build("suppliers", command.SupplierId, command.FileName);

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

        var attachment = new SupplierAttachment
        {
            Id = Guid.NewGuid(),
            SupplierId = command.SupplierId,
            FileName = command.FileName.Trim(),
            FileS3Key = key, // keep your field name
            DocumentType = command.DocumentType.Trim(),
            UploadedAt = now,
            UpdatedAt = now,
        };

        attachment.RaiseUploaded();

        _db.SupplierAttachments.Add(attachment);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Suppliers",
            "SupplierAttachment",
            attachment.Id,
            "SUPPLIER_ATTACHMENT_UPLOADED",
            string.Empty,
            $"supplierId={command.SupplierId}; attachmentId={attachment.Id}; fileName={attachment.FileName}; documentType={attachment.DocumentType}; key={attachment.FileS3Key}",
            ip,
            ua,
            ct
        );

        return Result.Success(attachment.Id);
    }
}
