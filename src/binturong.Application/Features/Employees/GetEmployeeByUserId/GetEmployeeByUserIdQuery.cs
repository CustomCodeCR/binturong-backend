using Application.Abstractions.Messaging;
using Application.ReadModels.Payroll;

namespace Application.Features.Employees.GetEmployeeByUserId;

public sealed record GetEmployeeByUserIdQuery(Guid UserId) : IQuery<EmployeeReadModel>;
