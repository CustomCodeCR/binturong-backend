namespace Api.Endpoints.Suppliers;

public sealed record UploadSupplierAttachmentRequest(
    string FileName,
    string FileS3Key,
    string DocumentType
);
