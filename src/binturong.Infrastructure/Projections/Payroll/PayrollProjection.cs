using Application.Abstractions.Projections;
using Application.ReadModels.Common;
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
        var col = _db.GetCollection<PayrollReadModel>(MongoCollections.Payrolls);

        var id = $"payroll:{e.PayrollId}";
        var filter = Builders<PayrollReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<PayrollReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.PayrollId, e.PayrollId)
            .Set(x => x.PeriodCode, e.PeriodCode)
            .Set(x => x.StartDate, ToUtcDate(e.StartDate))
            .Set(x => x.EndDate, ToUtcDate(e.EndDate))
            .Set(x => x.PayrollType, e.PayrollType)
            .Set(x => x.Status, e.Status)
            .Set(x => x.CreatedAtUtc, e.CreatedAtUtc)
            .Set(x => x.UpdatedAtUtc, e.UpdatedAtUtc)
            .SetOnInsert(x => x.Details, new List<PayrollDetailReadModel>());

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(PayrollCalculatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PayrollReadModel>(MongoCollections.Payrolls);

        await col.UpdateOneAsync(
            x => x.Id == $"payroll:{e.PayrollId}",
            Builders<PayrollReadModel>
                .Update.Set(x => x.Status, e.Status)
                .Set(x => x.UpdatedAtUtc, e.UpdatedAtUtc),
            new UpdateOptions { IsUpsert = true },
            ct
        );
    }

    public async Task ProjectAsync(PayrollClosedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PayrollReadModel>(MongoCollections.Payrolls);

        await col.UpdateOneAsync(
            x => x.Id == $"payroll:{e.PayrollId}",
            Builders<PayrollReadModel>
                .Update.Set(x => x.Status, e.Status)
                .Set(x => x.UpdatedAtUtc, e.UpdatedAtUtc),
            new UpdateOptions { IsUpsert = true },
            ct
        );
    }

    public async Task ProjectAsync(PayrollDetailCalculatedDomainEvent e, CancellationToken ct)
    {
        var payrollCol = _db.GetCollection<PayrollReadModel>(MongoCollections.Payrolls);
        var employeeCol = _db.GetCollection<EmployeeReadModel>(MongoCollections.Employees);

        var employee = await employeeCol
            .Find(x => x.EmployeeId == e.EmployeeId)
            .FirstOrDefaultAsync(ct);

        var id = $"payroll:{e.PayrollId}";
        var filter = Builders<PayrollReadModel>.Filter.Eq(x => x.Id, id);

        var ensure = Builders<PayrollReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.PayrollId, e.PayrollId);

        await payrollCol.UpdateOneAsync(filter, ensure, new UpdateOptions { IsUpsert = true }, ct);

        var pull = Builders<PayrollReadModel>.Update.PullFilter(
            x => x.Details,
            d => d.PayrollDetailId == e.PayrollDetailId
        );

        await payrollCol.UpdateOneAsync(filter, pull, new UpdateOptions { IsUpsert = false }, ct);

        var detail = new PayrollDetailReadModel
        {
            PayrollDetailId = e.PayrollDetailId,
            EmployeeId = e.EmployeeId,
            EmployeeName = employee?.FullName ?? string.Empty,
            GrossSalary = e.GrossSalary,
            OvertimeHours = e.OvertimeHours,
            CommissionAmount = e.CommissionAmount,
            Deductions = e.Deductions,
            EmployerContrib = e.EmployerContrib,
            NetSalary = e.NetSalary,
        };

        var push = Builders<PayrollReadModel>.Update.Push(x => x.Details, detail);

        await payrollCol.UpdateOneAsync(filter, push, new UpdateOptions { IsUpsert = false }, ct);

        await payrollCol.UpdateOneAsync(
            filter,
            Builders<PayrollReadModel>.Update.Set(x => x.UpdatedAtUtc, e.UpdatedAtUtc),
            new UpdateOptions { IsUpsert = false },
            ct
        );
    }

    public async Task ProjectAsync(PayrollCommissionAdjustedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<PayrollReadModel>(MongoCollections.Payrolls);

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

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private static DateTime ToUtcDate(DateOnly d) =>
        new(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
}
