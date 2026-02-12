using Application.Abstractions.Projections;
using Application.ReadModels.Payroll;
using Domain.PayrollDetails;
using Domain.Payrolls;
using MongoDB.Driver;

namespace Infrastructure.Projections.Payroll;

internal sealed class PayrollProjection
    : IProjector<PayrollCreatedDomainEvent>,
        IProjector<PayrollCalculatedDomainEvent>,
        IProjector<PayrollClosedDomainEvent>,
        IProjector<PayrollDetailCalculatedDomainEvent>,
        IProjector<PayrollCommissionAdjustedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public PayrollProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(PayrollCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PayrollReadModel>("payrolls");

        var rm = new PayrollReadModel
        {
            Id = $"payroll:{e.PayrollId}",
            PayrollId = e.PayrollId,
            PeriodCode = e.PeriodCode,
            StartDate = ToUtcDate(e.StartDate),
            EndDate = ToUtcDate(e.EndDate),
            PayrollType = e.PayrollType,
            Status = e.Status,
            CreatedAtUtc = e.CreatedAtUtc,
            UpdatedAtUtc = e.UpdatedAtUtc,
            Details = [],
        };

        await col.InsertOneAsync(rm, cancellationToken: ct);
    }

    public async Task ProjectAsync(PayrollCalculatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PayrollReadModel>("payrolls");
        await col.UpdateOneAsync(
            x => x.Id == $"payroll:{e.PayrollId}",
            Builders<PayrollReadModel>
                .Update.Set(x => x.Status, e.Status)
                .Set(x => x.UpdatedAtUtc, e.UpdatedAtUtc),
            cancellationToken: ct
        );
    }

    public async Task ProjectAsync(PayrollClosedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PayrollReadModel>("payrolls");
        await col.UpdateOneAsync(
            x => x.Id == $"payroll:{e.PayrollId}",
            Builders<PayrollReadModel>
                .Update.Set(x => x.Status, e.Status)
                .Set(x => x.UpdatedAtUtc, e.UpdatedAtUtc),
            cancellationToken: ct
        );
    }

    public async Task ProjectAsync(PayrollDetailCalculatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PayrollReadModel>("payrolls");
        var id = $"payroll:{e.PayrollId}";
        var filter = Builders<PayrollReadModel>.Filter.Eq(x => x.Id, id);

        var pull = Builders<PayrollReadModel>.Update.PullFilter(
            x => x.Details,
            d => d.PayrollDetailId == e.PayrollDetailId
        );

        await col.UpdateOneAsync(filter, pull, new UpdateOptions { IsUpsert = true }, ct);

        var detail = new PayrollDetailReadModel
        {
            PayrollDetailId = e.PayrollDetailId,
            EmployeeId = e.EmployeeId,
            EmployeeName = string.Empty,
            GrossSalary = e.GrossSalary,
            OvertimeHours = e.OvertimeHours,
            CommissionAmount = e.CommissionAmount,
            Deductions = e.Deductions,
            EmployerContrib = e.EmployerContrib,
            NetSalary = e.NetSalary,
        };

        var push = Builders<PayrollReadModel>.Update.Push(x => x.Details, detail);
        await col.UpdateOneAsync(filter, push, new UpdateOptions { IsUpsert = true }, ct);

        await col.UpdateOneAsync(
            filter,
            Builders<PayrollReadModel>.Update.Set(x => x.UpdatedAtUtc, e.UpdatedAtUtc),
            new UpdateOptions { IsUpsert = true },
            ct
        );
    }

    public async Task ProjectAsync(PayrollCommissionAdjustedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PayrollReadModel>("payrolls");
        var id = $"payroll:{e.PayrollId}";
        var filter = Builders<PayrollReadModel>.Filter.And(
            Builders<PayrollReadModel>.Filter.Eq(x => x.Id, id),
            Builders<PayrollReadModel>.Filter.ElemMatch(
                x => x.Details,
                d => d.PayrollDetailId == e.PayrollDetailId
            )
        );

        var update = Builders<PayrollReadModel>
            .Update.Set("Details.$.CommissionAmount", e.CommissionAmount)
            .Set(x => x.UpdatedAtUtc, e.UpdatedAtUtc);

        await col.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    private static DateTime ToUtcDate(DateOnly d) =>
        new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
}
