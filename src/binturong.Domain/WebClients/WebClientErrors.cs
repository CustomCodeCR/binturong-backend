using SharedKernel;

namespace Domain.WebClients;

public static class WebClientErrors
{
    public static Error NotFound(Guid webClientId) =>
        Error.NotFound(
            "WebClients.NotFound",
            $"The web client with the Id = '{webClientId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("WebClients.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error LoginEmailNotUnique = Error.Conflict(
        "WebClients.LoginEmailNotUnique",
        "The provided login email is not unique"
    );
}
