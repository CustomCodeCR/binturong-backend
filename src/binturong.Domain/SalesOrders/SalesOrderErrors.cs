using SharedKernel;

namespace Domain.SalesOrders;

public static class SalesOrderErrors
{
    public static Error NotFound(Guid salesOrderId) =>
        Error.NotFound(
            "SalesOrders.NotFound",
            $"The sales order with the Id = '{salesOrderId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("SalesOrders.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error CodeNotUnique = Error.Conflict(
        "SalesOrders.CodeNotUnique",
        "The provided sales order code is not unique"
    );
}
