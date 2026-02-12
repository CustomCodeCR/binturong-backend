namespace Application.ReadModels.Payroll;

public sealed class EmployeeReadModel
{
    public string Id { get; init; } = default!; // "employee:{EmployeeId}"
    public Guid EmployeeId { get; init; }

    public Guid? UserId { get; init; }
    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }

    public string FullName { get; init; } = default!;
    public string NationalId { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string JobTitle { get; init; } = default!;
    public decimal BaseSalary { get; init; }

    public DateTime HireDate { get; init; }
    public DateTime? TerminationDate { get; init; }

    public bool IsActive { get; init; }

    public IReadOnlyList<EmployeeHistoryReadModel> History { get; init; } = [];
}

public sealed class EmployeeHistoryReadModel
{
    public Guid HistoryId { get; init; }
    public string EventType { get; init; } = default!;
    public string Description { get; init; } = default!;
    public DateTime EventDate { get; init; }
}
