using Application.Abstractions.Messaging;
using Application.ReadModels.Services;

namespace Application.Features.ServiceOrders.GetServiceOrders;

public sealed record GetServiceOrdersQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Status,
    Guid? ContractId
) : IQuery<IReadOnlyList<ServiceOrderReadModel>>;
