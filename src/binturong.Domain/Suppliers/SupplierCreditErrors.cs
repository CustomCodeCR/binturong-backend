using SharedKernel;

namespace Domain.Suppliers;

public static class SupplierCreditErrors
{
    public static readonly Error CreditExceeded = Error.Failure(
        "Suppliers.CreditExceeded",
        "The credit limit exceeds the allowed maximum."
    );

    public static readonly Error Unauthorized = Error.Failure(
        "Suppliers.Credit.Unauthorized",
        "You are not authorized to modify supplier credit conditions."
    );
}
