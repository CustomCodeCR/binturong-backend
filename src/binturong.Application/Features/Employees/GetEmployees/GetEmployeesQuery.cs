using Application.Abstractions.Messaging;
using Application.ReadModels.Payroll;

namespace Application.Features.Employees.GetEmployees;

public sealed record GetEmployeesQuery(string? Search, Guid? UserId, int Skip = 0, int Take = 50)
    : IQuery<IReadOnlyList<EmployeeReadModel>>;
