using Domain.SalesOrderDetails;
using SharedKernel;

namespace Domain.SalesOrders;

public sealed class SalesOrder : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid? QuoteId { get; set; }
    public Guid ClientId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? SellerUserId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Taxes { get; set; }
    public decimal Discounts { get; set; }
    public decimal Total { get; set; }

    // Global discount
    public decimal GlobalDiscountPerc { get; set; }
    public decimal GlobalDiscountAmount { get; set; }
    public string GlobalDiscountReason { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Domain.Quotes.Quote? Quote { get; set; }
    public Domain.Clients.Client? Client { get; set; }
    public Domain.Branches.Branch? Branch { get; set; }

    public ICollection<SalesOrderDetail> Details { get; set; } = new List<SalesOrderDetail>();
    public ICollection<Domain.Invoices.Invoice> Invoices { get; set; } =
        new List<Domain.Invoices.Invoice>();
    public ICollection<Domain.Contracts.Contract> Contracts { get; set; } =
        new List<Domain.Contracts.Contract>();
    public ICollection<Domain.PurchaseOrders.PurchaseOrder> PurchaseOrders { get; set; } =
        new List<Domain.PurchaseOrders.PurchaseOrder>();

    public void RaiseCreated()
    {
        var createdAtUtc = CreatedAt == default ? DateTime.UtcNow : CreatedAt;
        var updatedAtUtc = UpdatedAt == default ? createdAtUtc : UpdatedAt;

        CreatedAt = createdAtUtc;
        UpdatedAt = updatedAtUtc;

        Raise(
            new SalesOrderCreatedDomainEvent(
                Id,
                Code,
                ClientId,
                BranchId,
                SellerUserId,
                OrderDate,
                Status,
                Currency,
                ExchangeRate,
                Subtotal,
                Taxes,
                Discounts,
                Total,
                QuoteId,
                string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                createdAtUtc,
                updatedAtUtc
            )
        );
    }

    public void RaiseConvertedFromQuote(Guid quoteId, DateTime updatedAtUtc)
    {
        UpdatedAt = updatedAtUtc;
        Raise(new SalesOrderConvertedFromQuoteDomainEvent(Id, quoteId, updatedAtUtc));
    }

    public void RaiseConfirmed(Guid sellerUserId, DateTime updatedAtUtc)
    {
        SellerUserId = sellerUserId;
        UpdatedAt = updatedAtUtc;
        Raise(new SalesOrderConfirmedDomainEvent(Id, sellerUserId, Total, updatedAtUtc));
    }

    public void RaiseDetailAdded(SalesOrderDetail d, DateTime updatedAtUtc)
    {
        UpdatedAt = updatedAtUtc;

        Raise(
            new SalesOrderDetailAddedDomainEvent(
                Id,
                d.Id,
                d.ItemType,
                d.ProductId,
                d.ServiceId,
                d.Quantity,
                d.UnitPrice,
                d.DiscountPerc,
                d.TaxPerc,
                d.LineTotal,
                updatedAtUtc
            )
        );
    }

    public Result ApplyLineDiscount(
        Guid salesOrderDetailId,
        decimal discountPerc,
        string reason,
        Guid appliedByUserId,
        DateTime appliedAtUtc,
        Domain.Discounts.DiscountPolicy policy
    )
    {
        var detail = Details.FirstOrDefault(x => x.Id == salesOrderDetailId);
        if (detail is null)
            return Result.Failure(Domain.Discounts.DiscountErrors.LineDiscountNotFound);

        if (discountPerc < 0 || discountPerc > 100)
            return Result.Failure(Domain.Discounts.DiscountErrors.PercentageInvalid);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(Domain.Discounts.DiscountErrors.ReasonRequired);

        var policyValidation = policy.ValidatePercentage(discountPerc);
        if (policyValidation.IsFailure)
            return policyValidation;

        var result = detail.ApplyDiscount(discountPerc, reason);
        if (result.IsFailure)
            return result;

        RecalculateTotals();

        Raise(
            new Domain.Discounts.SalesOrderLineDiscountAppliedDomainEvent(
                Id,
                detail.Id,
                detail.DiscountPerc,
                detail.DiscountAmount,
                detail.DiscountReason,
                appliedByUserId,
                appliedAtUtc
            )
        );

        UpdatedAt = appliedAtUtc;
        return Result.Success();
    }

    public Result ApplyLineDiscountApproved(
        Guid salesOrderDetailId,
        decimal discountPerc,
        string reason,
        Guid appliedByUserId,
        DateTime appliedAtUtc,
        Domain.Discounts.DiscountApprovalRequest approvalRequest
    )
    {
        var detail = Details.FirstOrDefault(x => x.Id == salesOrderDetailId);
        if (detail is null)
            return Result.Failure(Domain.Discounts.DiscountErrors.LineDiscountNotFound);

        if (!approvalRequest.IsApproved())
            return Result.Failure(Domain.Discounts.DiscountErrors.ApprovalRequestNotApproved);

        if (
            approvalRequest.SalesOrderId != Id
            || approvalRequest.SalesOrderDetailId != salesOrderDetailId
        )
            return Result.Failure(
                Error.Validation(
                    "Discounts.ApprovalRequestMismatch",
                    "Approval request does not match sales order line."
                )
            );

        var result = detail.ApplyDiscount(discountPerc, reason);
        if (result.IsFailure)
            return result;

        RecalculateTotals();

        Raise(
            new Domain.Discounts.SalesOrderLineDiscountAppliedDomainEvent(
                Id,
                detail.Id,
                detail.DiscountPerc,
                detail.DiscountAmount,
                detail.DiscountReason,
                appliedByUserId,
                appliedAtUtc
            )
        );

        UpdatedAt = appliedAtUtc;
        return Result.Success();
    }

    public Result RemoveLineDiscount(
        Guid salesOrderDetailId,
        Guid removedByUserId,
        DateTime removedAtUtc
    )
    {
        var detail = Details.FirstOrDefault(x => x.Id == salesOrderDetailId);
        if (detail is null)
            return Result.Failure(Domain.Discounts.DiscountErrors.LineDiscountNotFound);

        detail.RemoveDiscount();
        RecalculateTotals();

        Raise(
            new Domain.Discounts.SalesOrderLineDiscountRemovedDomainEvent(
                Id,
                detail.Id,
                removedByUserId,
                removedAtUtc
            )
        );

        UpdatedAt = removedAtUtc;
        return Result.Success();
    }

    public Result ApplyGlobalDiscount(
        decimal discountPerc,
        string reason,
        Guid appliedByUserId,
        DateTime appliedAtUtc,
        Domain.Discounts.DiscountPolicy policy
    )
    {
        if (!Details.Any())
            return Result.Failure(Domain.Discounts.DiscountErrors.SalesOrderWithoutItems);

        if (discountPerc < 0 || discountPerc > 100)
            return Result.Failure(Domain.Discounts.DiscountErrors.PercentageInvalid);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(Domain.Discounts.DiscountErrors.ReasonRequired);

        var policyValidation = policy.ValidatePercentage(discountPerc);
        if (policyValidation.IsFailure)
            return policyValidation;

        GlobalDiscountPerc = discountPerc;
        GlobalDiscountReason = reason.Trim();

        RecalculateTotals();

        Raise(
            new Domain.Discounts.SalesOrderGlobalDiscountAppliedDomainEvent(
                Id,
                GlobalDiscountPerc,
                GlobalDiscountAmount,
                GlobalDiscountReason,
                appliedByUserId,
                appliedAtUtc
            )
        );

        UpdatedAt = appliedAtUtc;
        return Result.Success();
    }

    public Result ApplyGlobalDiscountApproved(
        decimal discountPerc,
        string reason,
        Guid appliedByUserId,
        DateTime appliedAtUtc,
        Domain.Discounts.DiscountApprovalRequest approvalRequest
    )
    {
        if (!Details.Any())
            return Result.Failure(Domain.Discounts.DiscountErrors.SalesOrderWithoutItems);

        if (!approvalRequest.IsApproved())
            return Result.Failure(Domain.Discounts.DiscountErrors.ApprovalRequestNotApproved);

        if (approvalRequest.SalesOrderId != Id || approvalRequest.SalesOrderDetailId is not null)
            return Result.Failure(
                Error.Validation(
                    "Discounts.ApprovalRequestMismatch",
                    "Approval request does not match sales order total discount."
                )
            );

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(Domain.Discounts.DiscountErrors.ReasonRequired);

        GlobalDiscountPerc = discountPerc;
        GlobalDiscountReason = reason.Trim();

        RecalculateTotals();

        Raise(
            new Domain.Discounts.SalesOrderGlobalDiscountAppliedDomainEvent(
                Id,
                GlobalDiscountPerc,
                GlobalDiscountAmount,
                GlobalDiscountReason,
                appliedByUserId,
                appliedAtUtc
            )
        );

        UpdatedAt = appliedAtUtc;
        return Result.Success();
    }

    public Result RemoveGlobalDiscount(Guid removedByUserId, DateTime removedAtUtc)
    {
        if (GlobalDiscountPerc <= 0 && GlobalDiscountAmount <= 0)
            return Result.Failure(Domain.Discounts.DiscountErrors.GlobalDiscountNotFound);

        GlobalDiscountPerc = 0m;
        GlobalDiscountAmount = 0m;
        GlobalDiscountReason = string.Empty;

        RecalculateTotals();

        Raise(
            new Domain.Discounts.SalesOrderGlobalDiscountRemovedDomainEvent(
                Id,
                removedByUserId,
                removedAtUtc
            )
        );

        UpdatedAt = removedAtUtc;
        return Result.Success();
    }

    public Domain.Discounts.DiscountApprovalRequest CreateApprovalRequestForLineDiscount(
        Guid salesOrderDetailId,
        decimal requestedPercentage,
        Guid requestedByUserId,
        string reason,
        DateTime requestedAtUtc
    )
    {
        var detail = Details.First(x => x.Id == salesOrderDetailId);

        var req = new Domain.Discounts.DiscountApprovalRequest
        {
            Id = Guid.NewGuid(),
            SalesOrderId = Id,
            SalesOrderDetailId = detail.Id,
            Scope = "Line",
            RequestedPercentage = requestedPercentage,
            RequestedAmount = detail.GetBaseAmount() * (requestedPercentage / 100m),
            Reason = reason.Trim(),
            RequestedByUserId = requestedByUserId,
            RequestedAtUtc = requestedAtUtc,
            Status = "Pending",
        };

        req.RaiseCreated();
        return req;
    }

    public Domain.Discounts.DiscountApprovalRequest CreateApprovalRequestForGlobalDiscount(
        decimal requestedPercentage,
        Guid requestedByUserId,
        string reason,
        DateTime requestedAtUtc
    )
    {
        var subtotalBeforeGlobal = Details.Sum(x => x.GetBaseAmount() - x.DiscountAmount);

        var req = new Domain.Discounts.DiscountApprovalRequest
        {
            Id = Guid.NewGuid(),
            SalesOrderId = Id,
            SalesOrderDetailId = null,
            Scope = "Total",
            RequestedPercentage = requestedPercentage,
            RequestedAmount = subtotalBeforeGlobal * (requestedPercentage / 100m),
            Reason = reason.Trim(),
            RequestedByUserId = requestedByUserId,
            RequestedAtUtc = requestedAtUtc,
            Status = "Pending",
        };

        req.RaiseCreated();
        return req;
    }

    public void RecalculateTotals()
    {
        decimal subtotal = 0m;
        decimal lineDiscounts = 0m;
        decimal taxes = 0m;

        foreach (var line in Details)
        {
            line.RecalculateTotal();

            var lineBase = line.GetBaseAmount();
            var lineDiscount = line.DiscountAmount;
            var discountedBase = lineBase - lineDiscount;
            var lineTax = discountedBase * (line.TaxPerc / 100m);

            subtotal += lineBase;
            lineDiscounts += lineDiscount;
            taxes += lineTax;
        }

        var subtotalAfterLineDiscounts = subtotal - lineDiscounts;
        GlobalDiscountAmount = subtotalAfterLineDiscounts * (GlobalDiscountPerc / 100m);

        Discounts = lineDiscounts + GlobalDiscountAmount;
        Subtotal = subtotal;
        Taxes = taxes;
        Total = (subtotal - Discounts) + taxes;
    }
}
