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
}
