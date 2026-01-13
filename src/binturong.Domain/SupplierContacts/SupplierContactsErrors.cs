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
}
