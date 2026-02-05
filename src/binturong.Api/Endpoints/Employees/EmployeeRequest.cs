namespace Api.Endpoints.Employees;

public sealed record CreateEmployeeRequest(
    Guid? UserId,
    Guid? BranchId,
    string FullName,
    string NationalId,
    string JobTitle,
    decimal BaseSalary,
    DateOnly HireDate,
    DateOnly? TerminationDate = null,
    bool IsActive = true
);

public sealed record UpdateEmployeeRequest(
    Guid? UserId,
    Guid? BranchId,
    string FullName,
    string JobTitle,
    decimal BaseSalary,
    DateOnly? TerminationDate,
    bool IsActive
);
