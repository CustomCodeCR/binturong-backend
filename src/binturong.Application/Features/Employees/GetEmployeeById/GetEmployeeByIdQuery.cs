using Application.Abstractions.Messaging;
using Application.ReadModels.Payroll;

namespace Application.Features.Employees.GetEmployeeById;

public sealed record GetEmployeeByIdQuery(Guid EmployeeId) : IQuery<EmployeeReadModel>;
