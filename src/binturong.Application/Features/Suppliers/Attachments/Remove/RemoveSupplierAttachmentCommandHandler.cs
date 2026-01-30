using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.SupplierAttachments;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Attachments.Remove;

internal sealed class RemoveSupplierAttachmentCommandHandler
    : ICommandHandler<RemoveSupplierAttachmentCommand>
{
    private readonly IApplicationDbContext _db;

    public RemoveSupplierAttachmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(RemoveSupplierAttachmentCommand command, CancellationToken ct)
    {
        var supplierExists = await _db.Suppliers.AnyAsync(x => x.Id == command.SupplierId, ct);
        if (!supplierExists)
            return Result.Failure(SupplierErrors.NotFound(command.SupplierId));

        var attachment = await _db.SupplierAttachments.FirstOrDefaultAsync(
            x => x.Id == command.AttachmentId && x.SupplierId == command.SupplierId,
            ct
        );

        if (attachment is null)
            return Result.Failure(SupplierAttachmentErrors.NotFound(command.AttachmentId));

        attachment.RaiseDeleted();

        _db.SupplierAttachments.Remove(attachment);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
