using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Reports;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Reports.UpdateSchedule;

internal sealed class UpdateReportScheduleCommandHandler
    : ICommandHandler<UpdateReportScheduleCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateReportScheduleCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateReportScheduleCommand cmd, CancellationToken ct)
    {
        var schedule = await _db.ReportSchedules.FirstOrDefaultAsync(
            x => x.Id == cmd.ReportScheduleId,
            ct
        );
        if (schedule is null)
            return Result.Failure(ReportScheduleErrors.NotFound(cmd.ReportScheduleId));

        schedule.Name = cmd.Name?.Trim() ?? string.Empty;
        schedule.ReportType = cmd.ReportType?.Trim() ?? string.Empty;
        schedule.Frequency = cmd.Frequency?.Trim() ?? string.Empty;
        schedule.RecipientEmail = cmd.RecipientEmail?.Trim() ?? string.Empty;
        schedule.TimeOfDayUtc = cmd.TimeOfDayUtc;
        schedule.IsActive = cmd.IsActive;
        schedule.BranchId = cmd.BranchId;
        schedule.CategoryId = cmd.CategoryId;
        schedule.ClientId = cmd.ClientId;
        schedule.EmployeeId = cmd.EmployeeId;

        var validation = schedule.Validate();
        if (validation.IsFailure)
            return validation;

        schedule.RaiseUpdated();
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
