using SharedKernel;

namespace Domain.SupplierQuotes;

public sealed class SupplierQuote : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }
    public Guid? BranchId { get; set; }

    public DateTime RequestedAtUtc { get; set; }
    public DateTime? RespondedAtUtc { get; set; }

    public string Status { get; set; } = string.Empty; // Sent | Responded | Rejected
    public string Notes { get; set; } = string.Empty;
    public string SupplierMessage { get; set; } = string.Empty;
    public string RejectReason { get; set; } = string.Empty;

    public ICollection<SupplierQuoteLine> Lines { get; set; } = new List<SupplierQuoteLine>();

    // =========================
    // Domain events
    // =========================
    public void RaiseSent() =>
        Raise(
            new SupplierQuoteSentDomainEvent(
                Id,
                Code,
                SupplierId,
                BranchId,
                RequestedAtUtc,
                Status,
                string.IsNullOrWhiteSpace(Notes) ? null : Notes
            )
        );

    // =========================
    // Behavior
    // =========================
    public Result AddLine(Guid productId, decimal quantity)
    {
        if (productId == Guid.Empty)
            return Result.Failure(
                Error.Validation("SupplierQuotes.ProductRequired", "ProductId is required")
            );

        if (quantity <= 0)
            return Result.Failure(SupplierQuoteErrors.LineQuantityInvalid);

        var line = new SupplierQuoteLine
        {
            Id = Guid.NewGuid(),
            SupplierQuoteId = Id,
            ProductId = productId,
            Quantity = quantity,
        };

        Lines.Add(line);

        Raise(new SupplierQuoteLineAddedDomainEvent(Id, line.Id, line.ProductId, line.Quantity));

        return Result.Success();
    }

    public Result RegisterResponse(
        DateTime respondedAtUtc,
        string? supplierMessage,
        IReadOnlyList<SupplierQuoteResponseLineDto> lines
    )
    {
        if (!Status.Equals("Sent", StringComparison.OrdinalIgnoreCase))
            return Result.Failure(SupplierQuoteErrors.InvalidStatusTransition);

        if (lines is null || lines.Count == 0)
            return Result.Failure(
                Error.Validation(
                    "SupplierQuotes.NoResponseLines",
                    "Response must have at least one line"
                )
            );

        if (lines.Any(l => l.UnitPrice <= 0))
            return Result.Failure(SupplierQuoteErrors.ResponseLineUnitPriceInvalid);

        Status = "Responded";
        RespondedAtUtc = respondedAtUtc;
        SupplierMessage = supplierMessage?.Trim() ?? string.Empty;

        Raise(
            new SupplierQuoteRespondedDomainEvent(
                Id,
                respondedAtUtc,
                Status,
                string.IsNullOrWhiteSpace(SupplierMessage) ? null : SupplierMessage
            )
        );

        foreach (var l in lines)
        {
            Raise(
                new SupplierQuoteResponseLineRegisteredDomainEvent(
                    Id,
                    l.ProductId,
                    l.UnitPrice,
                    l.DiscountPerc,
                    l.TaxPerc,
                    string.IsNullOrWhiteSpace(l.Conditions) ? null : l.Conditions.Trim()
                )
            );
        }

        return Result.Success();
    }

    public Result Reject(string reason, DateTime rejectedAtUtc)
    {
        if (!Status.Equals("Sent", StringComparison.OrdinalIgnoreCase))
            return Result.Failure(SupplierQuoteErrors.InvalidStatusTransition);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(SupplierQuoteErrors.RejectReasonRequired);

        Status = "Rejected";
        RejectReason = reason.Trim();

        Raise(new SupplierQuoteRejectedDomainEvent(Id, rejectedAtUtc, Status, RejectReason));

        return Result.Success();
    }
}

public sealed record SupplierQuoteResponseLineDto(
    Guid ProductId,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc,
    string? Conditions
);
