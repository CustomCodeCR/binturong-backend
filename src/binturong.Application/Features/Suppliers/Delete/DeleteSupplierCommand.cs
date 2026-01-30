using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.Delete;

public sealed record DeleteSupplierCommand(Guid SupplierId) : ICommand;
