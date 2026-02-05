using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.Warehouses.GetWarehouseById;

public sealed record GetWarehouseByIdQuery(Guid WarehouseId) : IQuery<WarehouseReadModel>;
