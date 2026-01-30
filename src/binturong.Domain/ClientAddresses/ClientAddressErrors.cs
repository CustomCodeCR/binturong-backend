using SharedKernel;

namespace Domain.ClientAddresses;

public static class ClientAddressErrors
{
    public static Error NotFound(Guid addressId) =>
        Error.NotFound(
            "ClientAddresses.NotFound",
            $"The client address with the Id = '{addressId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ClientAddresses.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error ClientIdIsRequired = Error.Validation(
        "ClientAddresses.ClientIdIsRequired",
        "ClientId is required"
    );

    public static readonly Error AddressLineIsRequired = Error.Validation(
        "ClientAddresses.AddressLineIsRequired",
        "Address line is required"
    );

    public static readonly Error ProvinceIsRequired = Error.Validation(
        "ClientAddresses.ProvinceIsRequired",
        "Province is required"
    );

    public static readonly Error CantonIsRequired = Error.Validation(
        "ClientAddresses.CantonIsRequired",
        "Canton is required"
    );

    public static readonly Error DistrictIsRequired = Error.Validation(
        "ClientAddresses.DistrictIsRequired",
        "District is required"
    );

    public static readonly Error OnlyOnePrimaryAllowed = Error.Conflict(
        "ClientAddresses.OnlyOnePrimaryAllowed",
        "Only one primary address is allowed per client"
    );

    public static readonly Error CannotDeletePrimary = Error.Conflict(
        "ClientAddresses.CannotDeletePrimary",
        "The primary address cannot be deleted"
    );
}
