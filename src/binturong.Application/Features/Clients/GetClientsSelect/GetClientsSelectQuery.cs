using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Clients.GetClientsSelect;

public sealed record GetClientsSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
