using Application.Abstractions.Messaging;

namespace Application.Features.InventoryTransfers.Delete;

public sealed record DeleteInventoryTransferCommand(Guid TransferId) : ICommand;
