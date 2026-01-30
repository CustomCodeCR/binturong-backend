namespace Application.ReadModels.Common;

public sealed class AttachmentReadModel
{
    public string Id { get; init; } = default!;
    public Guid AttachmentId { get; init; }

    public string FileName { get; init; } = default!;
    public string FileS3Key { get; init; } = default!;
    public string? DocumentType { get; init; }
    public DateTime UploadedAt { get; init; }
}
