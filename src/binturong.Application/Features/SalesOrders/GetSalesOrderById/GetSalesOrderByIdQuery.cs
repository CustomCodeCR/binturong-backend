using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.SalesOrders.GetSalesOrderById;

public sealed record GetSalesOrderByIdQuery(Guid SalesOrderId) : IQuery<SalesOrderReadModel>;
