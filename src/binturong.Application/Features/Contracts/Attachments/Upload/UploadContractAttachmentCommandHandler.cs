using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Storage;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Contracts.Attachments.Upload;

internal sealed class UploadContractAttachmentCommandHandler
    : ICommandHandler<UploadContractAttachmentCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly IObjectStorage _storage;
    private readonly IObjectStorageKeyBuilder _keys;
    private readonly ICurrentUser _currentUser;

    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
    };

    private const long MaxBytes = 20 * 1024 * 1024; // 20MB

    public UploadContractAttachmentCommandHandler(
        IApplicationDbContext db,
        IObjectStorage storage,
        IObjectStorageKeyBuilder keys,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _storage = storage;
        _keys = keys;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(
        UploadContractAttachmentCommand cmd,
        CancellationToken ct
    )
    {
        if (cmd.ContractId == Guid.Empty)
            return Result.Failure<Guid>(
                Error.Validation("Contracts.IdRequired", "ContractId is required.")
            );

        if (cmd.Content is null || cmd.SizeBytes <= 0)
            return Result.Failure<Guid>(
                Error.Validation("Contracts.Attachments.Missing", "No file was provided.")
            );

        if (cmd.SizeBytes > MaxBytes)
            return Result.Failure<Guid>(
                Error.Validation("Contracts.Attachments.TooLarge", "File is too large.")
            );

        var ext = Path.GetExtension(cmd.FileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExt.Contains(ext))
            return Result.Failure<Guid>(
                Error.Validation(
                    "Contracts.Attachments.InvalidFormat",
                    $"File extension '{ext}' is not allowed."
                )
            );

        var exists = await _db.Contracts.AnyAsync(x => x.Id == cmd.ContractId, ct);
        if (!exists)
            return Result.Failure<Guid>(
                Error.NotFound("Contracts.NotFound", $"Contract '{cmd.ContractId}' not found.")
            );

        var key = _keys.Build("contracts", cmd.ContractId, cmd.FileName!);

        if (cmd.Content.CanSeek)
            cmd.Content.Position = 0;

        await _storage.PutAsync(
            key,
            cmd.Content,
            string.IsNullOrWhiteSpace(cmd.ContentType)
                ? "application/octet-stream"
                : cmd.ContentType.Trim(),
            ct
        );

        var attachmentId = Guid.NewGuid();

        var att = new ContractAttachment
        {
            Id = attachmentId,
            ContractId = cmd.ContractId,
            FileName = cmd.FileName!.Trim(),
            ContentType = (cmd.ContentType ?? "application/octet-stream").Trim(),
            SizeBytes = cmd.SizeBytes,
            StorageKey = key,
            UploadedAtUtc = DateTime.UtcNow,
            UploadedByUserId = _currentUser.UserId,
        };

        _db.ContractAttachments.Add(att);
        await _db.SaveChangesAsync(ct);

        return att.Id;
    }
}
