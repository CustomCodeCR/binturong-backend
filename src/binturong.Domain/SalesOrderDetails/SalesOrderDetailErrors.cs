using SharedKernel;

namespace Domain.SalesOrderDetails;

public static class SalesOrderDetailErrors
{
    public static Error NotFound(Guid salesOrderDetailId) =>
        Error.NotFound(
            "SalesOrderDetails.NotFound",
            $"The sales order detail with the Id = '{salesOrderDetailId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "SalesOrderDetails.Unauthorized",
            "You are not authorized to perform this action."
        );
}
