using Application.Abstractions.Messaging;
using Application.ReadModels.Services;

namespace Application.Features.ServiceOrders.GetServiceOrderById;

public sealed record GetServiceOrderByIdQuery(Guid ServiceOrderId) : IQuery<ServiceOrderReadModel>;
