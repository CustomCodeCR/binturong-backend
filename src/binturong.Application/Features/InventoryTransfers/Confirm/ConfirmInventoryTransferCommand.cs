using Application.Abstractions.Messaging;

namespace Application.Features.InventoryTransfers.Confirm;

public sealed record ConfirmInventoryTransferCommand(Guid TransferId, bool RequireApproval)
    : ICommand;
