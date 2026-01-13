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
}
