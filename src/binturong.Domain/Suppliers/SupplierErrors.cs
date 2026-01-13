using SharedKernel;

namespace Domain.Suppliers;

public static class SupplierErrors
{
    public static Error NotFound(Guid supplierId) =>
        Error.NotFound(
            "Suppliers.NotFound",
            $"The supplier with the Id = '{supplierId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("Suppliers.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Suppliers.EmailNotUnique",
        "The provided email is not unique"
    );

    public static readonly Error IdentificationNotUnique = Error.Conflict(
        "Suppliers.IdentificationNotUnique",
        "The provided identification is not unique"
    );
}
