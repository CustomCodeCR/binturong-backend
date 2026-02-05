using Application.Abstractions.Messaging;

namespace Application.Features.InventoryTransfers.Reject;

public sealed record RejectInventoryTransferCommand(
    Guid TransferId,
    Guid RejectedByUserId,
    string? Reason
) : ICommand;
