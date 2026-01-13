using SharedKernel;

namespace Domain.ClientContacts;

public static class ClientContactErrors
{
    public static Error NotFound(Guid contactId) =>
        Error.NotFound(
            "ClientContacts.NotFound",
            $"The client contact with the Id = '{contactId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ClientContacts.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error EmailNotUniqueForClient = Error.Conflict(
        "ClientContacts.EmailNotUniqueForClient",
        "The provided email is not unique for this client"
    );
}
