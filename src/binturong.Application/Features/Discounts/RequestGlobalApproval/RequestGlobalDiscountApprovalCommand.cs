using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.RequestGlobalApproval;

public sealed record RequestGlobalDiscountApprovalCommand(
    Guid SalesOrderId,
    decimal DiscountPerc,
    string Reason
) : ICommand<Guid>;
