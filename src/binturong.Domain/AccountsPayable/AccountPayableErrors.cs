using SharedKernel;

namespace Domain.AccountsPayable;

public static class AccountPayableErrors
{
    public static Error NotFound(Guid accountPayableId) =>
        Error.NotFound(
            "AccountsPayable.NotFound",
            $"The accounts payable record with the Id = '{accountPayableId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "AccountsPayable.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error InvalidPendingBalance = Error.Validation(
        "AccountsPayable.InvalidPendingBalance",
        "Pending balance cannot be negative"
    );
}
