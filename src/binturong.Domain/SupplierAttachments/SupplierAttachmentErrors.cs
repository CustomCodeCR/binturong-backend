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
}
