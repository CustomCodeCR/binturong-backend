namespace Api.Endpoints.Suppliers;

public sealed record CreateSupplierQuoteRequest(
    string Code,
    Guid SupplierId,
    Guid? BranchId,
    DateTime RequestedAtUtc,
    string? Notes,
    List<CreateSupplierQuoteLineRequest> Lines
);

public sealed record CreateSupplierQuoteLineRequest(Guid ProductId, decimal Quantity);

public sealed record RespondSupplierQuoteRequest(
    DateTime RespondedAtUtc,
    string? SupplierMessage,
    List<RespondSupplierQuoteLineRequest> Lines
);

public sealed record RespondSupplierQuoteLineRequest(
    Guid ProductId,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc,
    string? Conditions
);

public sealed record RejectSupplierQuoteRequest(string Reason, DateTime RejectedAtUtc);
