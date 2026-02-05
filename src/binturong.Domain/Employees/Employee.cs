using Domain.EmployeeHistory;
using SharedKernel;

namespace Domain.Employees;

public sealed class Employee : Entity
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? BranchId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public DateOnly HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public bool IsActive { get; set; }

    public Domain.Users.User? User { get; set; }
    public Domain.Branches.Branch? Branch { get; set; }
    public ICollection<EmployeeHistoryEntry> History { get; set; } =
        new List<EmployeeHistoryEntry>();

    public ICollection<Domain.PurchaseRequests.PurchaseRequest> PurchaseRequests { get; set; } =
        new List<Domain.PurchaseRequests.PurchaseRequest>();
    public ICollection<Domain.PayrollDetails.PayrollDetail> PayrollDetails { get; set; } =
        new List<Domain.PayrollDetails.PayrollDetail>();
    public ICollection<Domain.ServiceOrderTechnicians.ServiceOrderTechnician> ServiceOrderTechnicians { get; set; } =
        new List<Domain.ServiceOrderTechnicians.ServiceOrderTechnician>();

    public void RaiseCreated() =>
        Raise(
            new EmployeeCreatedDomainEvent(
                Id,
                FullName,
                NationalId,
                JobTitle,
                BaseSalary,
                BranchId,
                HireDate,
                IsActive
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new EmployeeUpdatedDomainEvent(Id, FullName, JobTitle, BaseSalary, BranchId, IsActive)
        );

    public void RaiseDeleted() => Raise(new EmployeeDeletedDomainEvent(Id));

    public Result RegisterCheckIn(bool hasOpenAttendance) =>
        RegisterCheckIn(hasOpenAttendance, DateTime.UtcNow);

    public Result RegisterCheckIn(bool hasOpenAttendance, DateTime occurredAtUtc)
    {
        if (!IsActive)
            return Result.Failure(EmployeeErrors.EmployeeInactive);

        if (hasOpenAttendance)
            return Result.Failure(EmployeeErrors.AttendanceAlreadyOpen);

        if (occurredAtUtc.Kind != DateTimeKind.Utc)
            occurredAtUtc = DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc);

        Raise(new EmployeeCheckInDomainEvent(Guid.NewGuid(), Id, occurredAtUtc));
        return Result.Success();
    }

    public Result RegisterCheckOut(bool hasOpenAttendance) =>
        RegisterCheckOut(hasOpenAttendance, DateTime.UtcNow);

    public Result RegisterCheckOut(bool hasOpenAttendance, DateTime occurredAtUtc)
    {
        if (!IsActive)
            return Result.Failure(EmployeeErrors.EmployeeInactive);

        if (!hasOpenAttendance)
            return Result.Failure(EmployeeErrors.AttendanceNotOpen);

        if (occurredAtUtc.Kind != DateTimeKind.Utc)
            occurredAtUtc = DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc);

        Raise(new EmployeeCheckOutDomainEvent(Guid.NewGuid(), Id, occurredAtUtc));
        return Result.Success();
    }
}
