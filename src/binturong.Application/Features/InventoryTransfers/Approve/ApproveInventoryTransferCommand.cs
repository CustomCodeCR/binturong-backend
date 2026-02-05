using Application.Abstractions.Messaging;

namespace Application.Features.InventoryTransfers.Approve;

public sealed record ApproveInventoryTransferCommand(Guid TransferId, Guid ApprovedByUserId)
    : ICommand;
