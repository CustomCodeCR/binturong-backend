using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.SupplierAttachments;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Attachments.Remove;

internal sealed class RemoveSupplierAttachmentCommandHandler
    : ICommandHandler<RemoveSupplierAttachmentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RemoveSupplierAttachmentCommandHandler(
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

    public async Task<Result> Handle(RemoveSupplierAttachmentCommand command, CancellationToken ct)
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
                "SUPPLIER_ATTACHMENT_REMOVE_FAILED",
                string.Empty,
                $"reason=supplier_not_found; supplierId={command.SupplierId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(SupplierErrors.NotFound(command.SupplierId));
        }

        var attachment = await _db.SupplierAttachments.FirstOrDefaultAsync(
            x => x.Id == command.AttachmentId && x.SupplierId == command.SupplierId,
            ct
        );

        if (attachment is null)
        {
            await _bus.AuditAsync(
                userId,
                "Suppliers",
                "SupplierAttachment",
                command.AttachmentId,
                "SUPPLIER_ATTACHMENT_REMOVE_FAILED",
                string.Empty,
                $"reason=attachment_not_found; attachmentId={command.AttachmentId}; supplierId={command.SupplierId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(SupplierAttachmentErrors.NotFound(command.AttachmentId));
        }

        var before =
            $"supplierId={attachment.SupplierId}; attachmentId={attachment.Id}; fileName={attachment.FileName}; documentType={attachment.DocumentType}";

        attachment.RaiseDeleted();

        _db.SupplierAttachments.Remove(attachment);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Suppliers",
            "SupplierAttachment",
            attachment.Id,
            "SUPPLIER_ATTACHMENT_REMOVED",
            before,
            $"attachmentId={attachment.Id}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
