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
}
