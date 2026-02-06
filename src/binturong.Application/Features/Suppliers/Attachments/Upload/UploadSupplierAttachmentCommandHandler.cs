using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
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
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UploadSupplierAttachmentCommandHandler(
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

        if (string.IsNullOrWhiteSpace(command.FileS3Key))
            return Result.Failure<Guid>(SupplierAttachmentErrors.FileS3KeyIsRequired);

        if (string.IsNullOrWhiteSpace(command.DocumentType))
            return Result.Failure<Guid>(SupplierAttachmentErrors.DocumentTypeIsRequired);

        var now = DateTime.UtcNow;

        var attachment = new SupplierAttachment
        {
            Id = Guid.NewGuid(),
            SupplierId = command.SupplierId,
            FileName = command.FileName.Trim(),
            FileS3Key = command.FileS3Key.Trim(),
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
            $"supplierId={command.SupplierId}; attachmentId={attachment.Id}; fileName={attachment.FileName}; documentType={attachment.DocumentType}",
            ip,
            ua,
            ct
        );

        return Result.Success(attachment.Id);
    }
}
