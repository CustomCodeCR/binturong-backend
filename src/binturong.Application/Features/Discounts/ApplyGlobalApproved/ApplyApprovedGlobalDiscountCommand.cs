using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.ApplyGlobalApproved;

public sealed record ApplyApprovedGlobalDiscountCommand(
    Guid SalesOrderId,
    decimal DiscountPerc,
    string Reason,
    Guid ApprovalRequestId
) : ICommand;
