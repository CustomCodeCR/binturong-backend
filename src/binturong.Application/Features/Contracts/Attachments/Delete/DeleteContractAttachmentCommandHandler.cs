using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Storage;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Contracts.Attachments.Delete;

internal sealed class DeleteContractAttachmentCommandHandler
    : ICommandHandler<DeleteContractAttachmentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IObjectStorage _storage;

    public DeleteContractAttachmentCommandHandler(IApplicationDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<Result> Handle(DeleteContractAttachmentCommand cmd, CancellationToken ct)
    {
        if (cmd.ContractId == Guid.Empty)
        {
            return Result.Failure(
                Error.Validation("Contracts.IdRequired", "ContractId is required.")
            );
        }

        if (cmd.AttachmentId == Guid.Empty)
        {
            return Result.Failure(
                Error.Validation("Contracts.Attachments.IdRequired", "AttachmentId is required.")
            );
        }

        var att = await _db.ContractAttachments.FirstOrDefaultAsync(
            x => x.ContractId == cmd.ContractId && x.Id == cmd.AttachmentId,
            ct
        );

        if (att is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "Contracts.Attachments.NotFound",
                    $"Attachment '{cmd.AttachmentId}' not found for contract '{cmd.ContractId}'."
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(att.StorageKey))
        {
            await _storage.DeleteAsync(att.StorageKey, ct);
        }

        att.RaiseDeleted();

        _db.ContractAttachments.Remove(att);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
