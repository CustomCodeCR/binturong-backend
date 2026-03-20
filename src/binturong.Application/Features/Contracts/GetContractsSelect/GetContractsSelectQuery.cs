using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Contracts.GetContractsSelect;

public sealed record GetContractsSelectQuery(string? Search)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
