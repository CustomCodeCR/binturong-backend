namespace Api.Endpoints.Invoices;

public sealed record CreateInvoiceLineRequest(
    Guid ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc
);

public sealed record CreateInvoiceRequest(
    Guid ClientId,
    Guid? BranchId,
    Guid? SalesOrderId,
    Guid? ContractId,
    DateTime IssueDate,
    string DocumentType, // "FE" etc
    string Currency,
    decimal ExchangeRate,
    string? Notes,
    IReadOnlyList<CreateInvoiceLineRequest> Lines
);

public sealed record UpdateInvoiceRequest(
    DateTime IssueDate,
    string DocumentType,
    string Currency,
    decimal ExchangeRate,
    string? Notes,
    string InternalStatus
);

public sealed record EmitInvoiceRequest(
    string Mode // "Normal" | "Contingency"
);

public sealed record ConvertQuoteToInvoiceRequest(
    Guid? BranchId,
    DateTime IssueDate,
    string DocumentType,
    string Mode // "Normal" | "Contingency"
);
