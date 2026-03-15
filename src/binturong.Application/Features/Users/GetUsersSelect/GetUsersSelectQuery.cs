using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Users.GetUsersSelect;

public sealed record GetUsersSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
