using Application.Abstractions.Messaging;
using Application.ReadModels.CRM;

namespace Application.Features.Suppliers.GetSupplierById;

public sealed record GetSupplierByIdQuery(Guid SupplierId) : IQuery<SupplierReadModel>;
