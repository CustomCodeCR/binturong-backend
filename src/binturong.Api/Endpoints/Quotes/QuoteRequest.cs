namespace Api.Endpoints.Quotes;

public sealed record CreateQuoteRequest(
    string Code,
    Guid ClientId,
    Guid? BranchId,
    DateTime IssueDate,
    DateTime ValidUntil,
    string Currency,
    decimal ExchangeRate,
    string? Notes
);

public sealed record AddQuoteDetailRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc
);

public sealed record RejectQuoteRequest(string Reason);
