using Application.Abstractions.Messaging;
using Application.ReadModels.Inventory;

namespace Application.Features.Branches.Inventory.GetBranchInventory;

public sealed record GetBranchInventoryQuery(Guid BranchId)
    : IQuery<IReadOnlyList<BranchInventoryItemReadModel>>;
