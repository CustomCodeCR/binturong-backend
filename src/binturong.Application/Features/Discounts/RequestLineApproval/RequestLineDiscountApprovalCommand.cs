using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.RequestLineApproval;

public sealed record RequestLineDiscountApprovalCommand(
    Guid SalesOrderId,
    Guid SalesOrderDetailId,
    decimal DiscountPerc,
    string Reason
) : ICommand<Guid>;
