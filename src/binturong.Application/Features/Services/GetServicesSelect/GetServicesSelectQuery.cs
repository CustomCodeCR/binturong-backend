using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Services.GetServicesSelect;

public sealed record GetServicesSelectQuery(string? Search)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
