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

    public static readonly Error ClientIdIsRequired = Error.Validation(
        "ClientContacts.ClientIdIsRequired",
        "ClientId is required"
    );

    public static readonly Error NameIsRequired = Error.Validation(
        "ClientContacts.NameIsRequired",
        "Contact name is required"
    );

    public static readonly Error EmailIsRequired = Error.Validation(
        "ClientContacts.EmailIsRequired",
        "Contact email is required"
    );

    public static readonly Error OnlyOnePrimaryAllowed = Error.Conflict(
        "ClientContacts.OnlyOnePrimaryAllowed",
        "Only one primary contact is allowed per client"
    );

    public static readonly Error CannotDeletePrimary = Error.Conflict(
        "ClientContacts.CannotDeletePrimary",
        "The primary contact cannot be deleted"
    );
}
