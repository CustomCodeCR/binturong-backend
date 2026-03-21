using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Reports;
using SharedKernel;

namespace Application.Features.Reports.CreateSchedule;

internal sealed class CreateReportScheduleCommandHandler
    : ICommandHandler<CreateReportScheduleCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateReportScheduleCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateReportScheduleCommand cmd, CancellationToken ct)
    {
        var schedule = new ReportSchedule
        {
            Id = Guid.NewGuid(),
            Name = cmd.Name?.Trim() ?? string.Empty,
            ReportType = cmd.ReportType?.Trim() ?? string.Empty,
            Frequency = cmd.Frequency?.Trim() ?? string.Empty,
            RecipientEmail = cmd.RecipientEmail?.Trim() ?? string.Empty,
            TimeOfDayUtc = cmd.TimeOfDayUtc,
            IsActive = cmd.IsActive,
            BranchId = cmd.BranchId,
            CategoryId = cmd.CategoryId,
            ClientId = cmd.ClientId,
            EmployeeId = cmd.EmployeeId,
        };

        var validation = schedule.Validate();
        if (validation.IsFailure)
            return Result.Failure<Guid>(validation.Error);

        schedule.RaiseCreated();

        _db.ReportSchedules.Add(schedule);
        await _db.SaveChangesAsync(ct);

        return Result.Success(schedule.Id);
    }
}
