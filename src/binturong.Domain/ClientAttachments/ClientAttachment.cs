using SharedKernel;

namespace Domain.ClientAttachments;

public sealed class ClientAttachment : Entity
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileS3Key { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    public Domain.Clients.Client? Client { get; set; }
}
