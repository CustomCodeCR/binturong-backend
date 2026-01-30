using SharedKernel;

namespace Domain.SupplierAttachments;

public static class SupplierAttachmentErrors
{
    public static Error NotFound(Guid attachmentId) =>
        Error.NotFound(
            "SupplierAttachments.NotFound",
            $"The supplier attachment with the Id = '{attachmentId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "SupplierAttachments.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error SupplierIdIsRequired = Error.Validation(
        "SupplierAttachments.SupplierIdIsRequired",
        "SupplierId is required"
    );

    public static readonly Error FileNameIsRequired = Error.Validation(
        "SupplierAttachments.FileNameIsRequired",
        "File name is required"
    );

    public static readonly Error FileS3KeyIsRequired = Error.Validation(
        "SupplierAttachments.FileS3KeyIsRequired",
        "File S3 key is required"
    );

    public static readonly Error DocumentTypeIsRequired = Error.Validation(
        "SupplierAttachments.DocumentTypeIsRequired",
        "Document type is required"
    );
}
