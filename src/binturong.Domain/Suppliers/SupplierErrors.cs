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

    public static readonly Error EmailIsRequired = Error.Validation(
        "Suppliers.EmailIsRequired",
        "Email is required"
    );

    public static readonly Error IdentificationIsRequired = Error.Validation(
        "Suppliers.IdentificationIsRequired",
        "Identification is required"
    );

    public static readonly Error TradeNameIsRequired = Error.Validation(
        "Suppliers.TradeNameIsRequired",
        "Trade Name is required"
    );

    public static readonly Error PhoneIsRequired = Error.Validation(
        "Suppliers.PhoneIsRequired",
        "Phone is required"
    );

    public static readonly Error LegalNameIsRequired = Error.Validation(
        "Suppliers.LegalNameIsRequired",
        "Legal Name is required"
    );
}
