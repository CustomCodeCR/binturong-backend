using SharedKernel;

namespace Domain.Employees;

public sealed record EmployeeCreatedDomainEvent(
    Guid EmployeeId,
    string FullName,
    string NationalId,
    string Email,
    string JobTitle,
    decimal BaseSalary,
    Guid? BranchId,
    DateOnly HireDate,
    bool IsActive
) : IDomainEvent;

public sealed record EmployeeUpdatedDomainEvent(
    Guid EmployeeId,
    string FullName,
    string Email,
    string JobTitle,
    decimal BaseSalary,
    Guid? BranchId,
    bool IsActive
) : IDomainEvent;

public sealed record EmployeeDeletedDomainEvent(Guid EmployeeId) : IDomainEvent;
