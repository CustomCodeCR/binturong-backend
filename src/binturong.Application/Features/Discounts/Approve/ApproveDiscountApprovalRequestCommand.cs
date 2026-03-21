using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.Approve;

public sealed record ApproveDiscountApprovalRequestCommand(Guid ApprovalRequestId) : ICommand;
