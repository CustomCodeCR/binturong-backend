using Application.Abstractions.Messaging;

namespace Application.Features.Employees.Create;

public sealed record CreateEmployeeCommand(
    Guid? UserId,
    Guid? BranchId,
    string FullName,
    string NationalId,
    string JobTitle,
    decimal BaseSalary,
    DateOnly HireDate,
    DateOnly? TerminationDate = null,
    bool IsActive = true
) : ICommand<Guid>;
