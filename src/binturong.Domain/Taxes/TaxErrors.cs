using SharedKernel;

namespace Domain.Taxes;

public static class TaxErrors
{
    public static Error NotFound(Guid taxId) =>
        Error.NotFound("Taxes.NotFound", $"The tax with the Id = '{taxId}' was not found");

    public static Error Unauthorized() =>
        Error.Failure("Taxes.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error CodeNotUnique = Error.Conflict(
        "Taxes.CodeNotUnique",
        "The provided tax code is not unique"
    );
    public static readonly Error NameIsRequired = Error.Validation(
        "Taxes.NameIsRequired",
        "Tax name is required"
    );
    public static readonly Error CodeIsRequired = Error.Validation(
        "Taxes.CodeIsRequired",
        "Tax code is required"
    );
    public static readonly Error InvalidPercentage = Error.Validation(
        "Taxes.InvalidPercentage",
        "Tax percentage must be between 0 and 100"
    );
    public static readonly Error InactiveTax = Error.Failure(
        "Taxes.Inactive",
        "The tax is inactive"
    );
    public static readonly Error CannotDeleteInUse = Error.Conflict(
        "Taxes.InUse",
        "The tax cannot be deleted because it is in use"
    );
}
