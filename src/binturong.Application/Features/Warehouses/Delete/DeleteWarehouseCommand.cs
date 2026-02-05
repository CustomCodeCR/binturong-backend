using Application.Abstractions.Messaging;

namespace Application.Features.Warehouses.Delete;

public sealed record DeleteWarehouseCommand(Guid WarehouseId) : ICommand;
