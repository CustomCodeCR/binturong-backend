using SharedKernel;

namespace Domain.Employees;

public sealed record EmployeeCreatedDomainEvent(
    Guid EmployeeId,
    string FullName,
    Guid? UserId,
    string NationalId,
    string Email,
    string JobTitle,
    decimal BaseSalary,
    Guid? BranchId,
    DateOnly HireDate,
    DateOnly? TerminationDate,
    bool IsActive
) : IDomainEvent;

public sealed record EmployeeUpdatedDomainEvent(
    Guid EmployeeId,
    string FullName,
    Guid? UserId,
    string NationalId,
    string Email,
    string JobTitle,
    decimal BaseSalary,
    Guid? BranchId,
    DateOnly? TerminationDate,
    bool IsActive
) : IDomainEvent;

public sealed record EmployeeDeletedDomainEvent(Guid EmployeeId) : IDomainEvent;
