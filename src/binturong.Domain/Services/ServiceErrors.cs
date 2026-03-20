using SharedKernel;

namespace Domain.Services;

public static class ServiceErrors
{
    public static readonly Error CodeRequired = Error.Validation(
        "Services.CodeRequired",
        "Code is required."
    );

    public static readonly Error NameRequired = Error.Validation(
        "Services.NameRequired",
        "Name is required."
    );

    public static readonly Error NameDuplicated = Error.Validation(
        "Services.NameDuplicated",
        "A service with the same name already exists."
    );

    public static readonly Error CategoryRequired = Error.Validation(
        "Services.CategoryRequired",
        "CategoryId is required."
    );

    public static readonly Error BaseRateInvalid = Error.Validation(
        "Services.BaseRateInvalid",
        "BaseRate must be greater than 0."
    );

    public static readonly Error StandardTimeInvalid = Error.Validation(
        "Services.StandardTimeInvalid",
        "StandardTimeMin must be greater than 0."
    );

    public static readonly Error CategoryProtected = Error.Validation(
        "Services.CategoryProtected",
        "Protected category cannot be changed."
    );

    public static readonly Error NotAvailable = Error.Validation(
        "Services.NotAvailable",
        "Service is not available for association."
    );

    public static readonly Error AvailabilityStatusInvalid = Error.Validation(
        "Services.AvailabilityStatusInvalid",
        "AvailabilityStatus is invalid."
    );

    public static Error NotFound(Guid id) =>
        Error.NotFound("Services.NotFound", $"Service '{id}' not found.");

    public static Error CategoryNotFound(Guid categoryId) =>
        Error.NotFound("Services.CategoryNotFound", $"Category '{categoryId}' not found.");
}
