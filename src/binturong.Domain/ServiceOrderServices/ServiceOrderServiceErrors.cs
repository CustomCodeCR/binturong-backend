using SharedKernel;

namespace Domain.ServiceOrderServices;

public static class ServiceOrderServiceErrors
{
    public static Error NotFound(Guid serviceOrderServiceId) =>
        Error.NotFound(
            "ServiceOrderServices.NotFound",
            $"The service order service with the Id = '{serviceOrderServiceId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ServiceOrderServices.Unauthorized",
            "You are not authorized to perform this action."
        );
}
