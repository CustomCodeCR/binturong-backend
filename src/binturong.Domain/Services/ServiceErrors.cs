using SharedKernel;

namespace Domain.Services;

public static class ServiceErrors
{
    public static Error NotFound(Guid serviceId) =>
        Error.NotFound(
            "Services.NotFound",
            $"The service with the Id = '{serviceId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("Services.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error CodeNotUnique = Error.Conflict(
        "Services.CodeNotUnique",
        "The provided service code is not unique"
    );
}
