using SharedKernel;

namespace Domain.Clients;

public static class ClientErrors
{
    public static Error NotFound(Guid clientId) =>
        Error.NotFound("Clients.NotFound", $"The client with the Id = '{clientId}' was not found");

    public static Error Unauthorized() =>
        Error.Failure("Clients.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Clients.EmailNotUnique",
        "The provided email is not unique"
    );

    public static readonly Error IdentificationNotUnique = Error.Conflict(
        "Clients.IdentificationNotUnique",
        "The provided identification is not unique"
    );
}
