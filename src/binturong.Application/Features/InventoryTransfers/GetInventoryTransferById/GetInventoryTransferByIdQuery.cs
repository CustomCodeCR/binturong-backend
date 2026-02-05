using Application.Abstractions.Messaging;
using Application.ReadModels.Inventory;

namespace Application.Features.InventoryTransfers.GetInventoryTransferById;

public sealed record GetInventoryTransferByIdQuery(Guid TransferId)
    : IQuery<InventoryTransferReadModel>;
