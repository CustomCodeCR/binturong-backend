using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.SupplierAttachments;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Attachments.Upload;

internal sealed class UploadSupplierAttachmentCommandHandler
    : ICommandHandler<UploadSupplierAttachmentCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public UploadSupplierAttachmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(
        UploadSupplierAttachmentCommand command,
        CancellationToken ct
    )
    {
        var supplierExists = await _db.Suppliers.AnyAsync(x => x.Id == command.SupplierId, ct);
        if (!supplierExists)
            return Result.Failure<Guid>(SupplierErrors.NotFound(command.SupplierId));

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

        return Result.Success(attachment.Id);
    }
}
