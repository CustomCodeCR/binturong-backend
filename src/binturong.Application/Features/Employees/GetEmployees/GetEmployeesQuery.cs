using Application.Abstractions.Messaging;
using Application.ReadModels.Payroll;

namespace Application.Features.Employees.GetEmployees;

public sealed record GetEmployeesQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<EmployeeReadModel>>
{
    public int Take => PageSize;
    public int Skip => (Page - 1) * PageSize;
}
