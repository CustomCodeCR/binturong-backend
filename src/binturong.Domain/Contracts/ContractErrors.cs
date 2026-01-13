using SharedKernel;

namespace Domain.Contracts;

public static class ContractErrors
{
    public static Error NotFound(Guid contractId) =>
        Error.NotFound(
            "Contracts.NotFound",
            $"The contract with the Id = '{contractId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("Contracts.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error CodeNotUnique = Error.Conflict(
        "Contracts.CodeNotUnique",
        "The provided contract code is not unique"
    );
}
