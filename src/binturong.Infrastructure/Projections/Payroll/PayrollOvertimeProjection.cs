using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Payroll;
using Domain.PayrollOvertimes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Payroll;

internal sealed class PayrollOvertimeProjection
    : IProjector<PayrollOvertimeRegisteredDomainEvent>,
        IProjector<PayrollOvertimeDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public PayrollOvertimeProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(PayrollOvertimeRegisteredDomainEvent e, CancellationToken ct)
    {
        var overtimeCol = _db.GetCollection<PayrollOvertimeReadModel>(
            MongoCollections.PayrollOvertimes
        );
        var employeeCol = _db.GetCollection<EmployeeReadModel>(MongoCollections.Employees);

        var employee = await employeeCol
            .Find(x => x.EmployeeId == e.EmployeeId)
            .FirstOrDefaultAsync(ct);

        var id = $"payroll_overtime:{e.OvertimeId}";
        var filter = Builders<PayrollOvertimeReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<PayrollOvertimeReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.OvertimeId, e.OvertimeId)
            .Set(x => x.EmployeeId, e.EmployeeId)
            .Set(x => x.EmployeeName, employee?.FullName ?? string.Empty)
            .Set(x => x.WorkDateUtc, ToUtcDate(e.WorkDate))
            .Set(x => x.Hours, e.Hours)
            .Set(x => x.CreatedAtUtc, e.CreatedAtUtc);

        await overtimeCol.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(PayrollOvertimeDeletedDomainEvent e, CancellationToken ct)
    {
        var overtimeCol = _db.GetCollection<PayrollOvertimeReadModel>(
            MongoCollections.PayrollOvertimes
        );

        await overtimeCol.DeleteOneAsync(x => x.Id == $"payroll_overtime:{e.OvertimeId}", ct);
    }

    private static DateTime ToUtcDate(DateOnly d) =>
        new(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
}
