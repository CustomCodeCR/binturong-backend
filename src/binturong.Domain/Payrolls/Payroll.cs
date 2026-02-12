using SharedKernel;

namespace Domain.Payrolls;

public sealed class Payroll : Entity
{
    public Guid Id { get; set; }
    public string PeriodCode { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string PayrollType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<Domain.PayrollDetails.PayrollDetail> Details { get; set; } =
        new List<Domain.PayrollDetails.PayrollDetail>();

    public void RaiseCreated() =>
        Raise(
            new PayrollCreatedDomainEvent(
                Id,
                PeriodCode,
                StartDate,
                EndDate,
                PayrollType,
                Status,
                CreatedAtUtc,
                UpdatedAtUtc
            )
        );

    public void RaiseCalculated() =>
        Raise(new PayrollCalculatedDomainEvent(Id, Status, UpdatedAtUtc));

    public void RaiseClosed() => Raise(new PayrollClosedDomainEvent(Id, Status, UpdatedAtUtc));
}
