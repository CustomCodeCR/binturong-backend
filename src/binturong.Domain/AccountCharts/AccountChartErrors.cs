using SharedKernel;

namespace Domain.AccountsChart;

public static class AccountChartErrors
{
    public static Error NotFound(Guid accountId) =>
        Error.NotFound(
            "AccountsChart.NotFound",
            $"The account with the Id = '{accountId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "AccountsChart.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error CodeNotUnique = Error.Conflict(
        "AccountsChart.CodeNotUnique",
        "The provided account code is not unique"
    );
}
