using SharedKernel;

namespace Domain.InvoiceDetails;

public static class InvoiceDetailErrors
{
    public static Error NotFound(Guid invoiceDetailId) =>
        Error.NotFound(
            "InvoiceDetails.NotFound",
            $"The invoice detail with the Id = '{invoiceDetailId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "InvoiceDetails.Unauthorized",
            "You are not authorized to perform this action."
        );
}
