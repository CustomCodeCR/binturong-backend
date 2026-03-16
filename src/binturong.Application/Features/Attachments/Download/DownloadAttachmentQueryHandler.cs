using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Storage;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Attachments.Download;

internal sealed class DownloadAttachmentQueryHandler
    : IQueryHandler<DownloadAttachmentQuery, DownloadAttachmentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IObjectStorage _storage;

    public DownloadAttachmentQueryHandler(IApplicationDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<Result<DownloadAttachmentResponse>> Handle(
        DownloadAttachmentQuery query,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(query.Module))
        {
            return Result.Failure<DownloadAttachmentResponse>(
                Error.Validation("Attachments.Module.Required", "Attachment module is required.")
            );
        }

        var module = query.Module.Trim().ToLowerInvariant();

        return module switch
        {
            "suppliers" => await HandleSupplierAttachment(query.AttachmentId, ct),
            "customers" or "clients" => await HandleClientAttachment(query.AttachmentId, ct),
            _ => Result.Failure<DownloadAttachmentResponse>(
                Error.Validation(
                    "Attachments.Module.Invalid",
                    $"Unsupported attachment module '{query.Module}'."
                )
            ),
        };
    }

    private async Task<Result<DownloadAttachmentResponse>> HandleSupplierAttachment(
        Guid attachmentId,
        CancellationToken ct
    )
    {
        var attachment = await _db
            .SupplierAttachments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == attachmentId, ct);

        if (attachment is null)
        {
            return Result.Failure<DownloadAttachmentResponse>(
                Error.NotFound("Attachments.NotFound", "Attachment was not found.")
            );
        }

        var url = await _storage.GetReadUrlAsync(attachment.FileS3Key, ct);

        return Result.Success(new DownloadAttachmentResponse(attachment.FileName, url));
    }

    private async Task<Result<DownloadAttachmentResponse>> HandleClientAttachment(
        Guid attachmentId,
        CancellationToken ct
    )
    {
        var attachment = await _db
            .ClientAttachments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == attachmentId, ct);

        if (attachment is null)
        {
            return Result.Failure<DownloadAttachmentResponse>(
                Error.NotFound("Attachments.NotFound", "Attachment was not found.")
            );
        }

        var url = await _storage.GetReadUrlAsync(attachment.FileS3Key, ct);

        return Result.Success(new DownloadAttachmentResponse(attachment.FileName, url));
    }
}
