using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.ApplyLineApproved;

public sealed record ApplyApprovedLineDiscountCommand(
    Guid SalesOrderId,
    Guid SalesOrderDetailId,
    decimal DiscountPerc,
    string Reason,
    Guid ApprovalRequestId
) : ICommand;
