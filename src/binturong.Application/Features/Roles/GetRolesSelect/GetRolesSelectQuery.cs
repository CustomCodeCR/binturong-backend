using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Roles.GetRolesSelect;

public sealed record GetRolesSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
