using SharedKernel;

namespace Domain.SupplierContacts;

public static class SupplierContactErrors
{
    public static Error NotFound(Guid contactId) =>
        Error.NotFound(
            "SupplierContacts.NotFound",
            $"The supplier contact with the Id = '{contactId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "SupplierContacts.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error EmailNotUniqueForSupplier = Error.Conflict(
        "SupplierContacts.EmailNotUniqueForSupplier",
        "The provided email is not unique for this supplier"
    );

    public static readonly Error SupplierIdIsRequired = Error.Validation(
        "SupplierContacts.SupplierIdIsRequired",
        "SupplierId is required"
    );

    public static readonly Error NameIsRequired = Error.Validation(
        "SupplierContacts.NameIsRequired",
        "Contact name is required"
    );

    public static readonly Error EmailIsRequired = Error.Validation(
        "SupplierContacts.EmailIsRequired",
        "Contact email is required"
    );

    public static readonly Error OnlyOnePrimaryAllowed = Error.Conflict(
        "SupplierContacts.OnlyOnePrimaryAllowed",
        "Only one primary contact is allowed per client"
    );

    public static readonly Error CannotDeletePrimary = Error.Conflict(
        "SupplierContacts.CannotDeletePrimary",
        "The primary contact cannot be deleted"
    );
}
