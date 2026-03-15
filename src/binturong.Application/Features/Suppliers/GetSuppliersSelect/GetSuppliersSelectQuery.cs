using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Suppliers.GetSuppliersSelect;

public sealed record GetSuppliersSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
