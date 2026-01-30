using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ClientAttachments;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Attachments.Upload;

internal sealed class UploadClientAttachmentCommandHandler
    : ICommandHandler<UploadClientAttachmentCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public UploadClientAttachmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(
        UploadClientAttachmentCommand command,
        CancellationToken ct
    )
    {
        var clientExists = await _db.Clients.AnyAsync(x => x.Id == command.ClientId, ct);
        if (!clientExists)
            return Result.Failure<Guid>(ClientErrors.NotFound(command.ClientId));

        if (string.IsNullOrWhiteSpace(command.FileName))
            return Result.Failure<Guid>(ClientAttachmentErrors.FileNameIsRequired);

        if (string.IsNullOrWhiteSpace(command.FileS3Key))
            return Result.Failure<Guid>(ClientAttachmentErrors.FileS3KeyIsRequired);

        if (string.IsNullOrWhiteSpace(command.DocumentType))
            return Result.Failure<Guid>(ClientAttachmentErrors.DocumentTypeIsRequired);

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

        return Result.Success(attachment.Id);
    }
}
