using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Employees.GetEmployeesSelect;

public sealed record GetEmployeesSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
