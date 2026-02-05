using Application.Abstractions.Messaging;

namespace Application.Features.Employees.Update;

public sealed record UpdateEmployeeCommand(
    Guid EmployeeId,
    Guid? UserId,
    Guid? BranchId,
    string FullName,
    string JobTitle,
    decimal BaseSalary,
    DateOnly? TerminationDate,
    bool IsActive
) : ICommand;
