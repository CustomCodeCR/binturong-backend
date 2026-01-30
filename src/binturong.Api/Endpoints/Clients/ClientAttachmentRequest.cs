namespace Api.Endpoints.Clients;

public sealed record UploadClientAttachmentRequest(
    string FileName,
    string FileS3Key,
    string DocumentType
);
