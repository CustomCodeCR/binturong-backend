using Application.Abstractions.Messaging;
using Application.ReadModels.Services;

namespace Application.Features.Services.GetServices;

public sealed record GetServicesQuery(
    int Page,
    int PageSize,
    string? Search,
    Guid? CategoryId,
    bool? IsActive
) : IQuery<IReadOnlyList<ServiceReadModel>>;
