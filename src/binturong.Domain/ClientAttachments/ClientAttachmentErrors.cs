using SharedKernel;

namespace Domain.ClientAttachments;

public static class ClientAttachmentErrors
{
    public static Error NotFound(Guid attachmentId) =>
        Error.NotFound(
            "ClientAttachments.NotFound",
            $"The client attachment with the Id = '{attachmentId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ClientAttachments.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error ClientIdIsRequired = Error.Validation(
        "ClientAttachments.ClientIdIsRequired",
        "ClientId is required"
    );

    public static readonly Error FileNameIsRequired = Error.Validation(
        "ClientAttachments.FileNameIsRequired",
        "File name is required"
    );

    public static readonly Error FileS3KeyIsRequired = Error.Validation(
        "ClientAttachments.FileS3KeyIsRequired",
        "File S3 key is required"
    );

    public static readonly Error DocumentTypeIsRequired = Error.Validation(
        "ClientAttachments.DocumentTypeIsRequired",
        "Document type is required"
    );
}
