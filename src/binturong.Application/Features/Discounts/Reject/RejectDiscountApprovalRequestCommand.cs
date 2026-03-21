using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.Reject;

public sealed record RejectDiscountApprovalRequestCommand(
    Guid ApprovalRequestId,
    string RejectionReason
) : ICommand;
