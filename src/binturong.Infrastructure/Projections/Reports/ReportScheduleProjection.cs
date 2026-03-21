using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Reports;
using Domain.Reports;
using MongoDB.Driver;

namespace Infrastructure.Projections.Reports;

internal sealed class ReportScheduleProjection
    : IProjector<ReportScheduleCreatedDomainEvent>,
        IProjector<ReportScheduleUpdatedDomainEvent>,
        IProjector<ReportScheduleDeletedDomainEvent>,
        IProjector<ReportScheduleExecutionSucceededDomainEvent>,
        IProjector<ReportScheduleExecutionFailedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public ReportScheduleProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(ReportScheduleCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ReportScheduleReadModel>(MongoCollections.ReportSchedules);

        var id = $"report_schedule:{e.ReportScheduleId}";
        var filter = Builders<ReportScheduleReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ReportScheduleReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ReportScheduleId, e.ReportScheduleId)
            .Set(x => x.Name, e.Name)
            .Set(x => x.ReportType, e.ReportType)
            .Set(x => x.Frequency, e.Frequency)
            .Set(x => x.RecipientEmail, e.RecipientEmail)
            .Set(x => x.TimeOfDayUtc, e.TimeOfDayUtc)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.CategoryId, e.CategoryId)
            .Set(x => x.ClientId, e.ClientId)
            .Set(x => x.EmployeeId, e.EmployeeId)
            .Set(x => x.LastSentAtUtc, null)
            .Set(x => x.LastAttemptAtUtc, null)
            .Set(x => x.LastError, null);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ReportScheduleUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ReportScheduleReadModel>(MongoCollections.ReportSchedules);

        var id = $"report_schedule:{e.ReportScheduleId}";
        var filter = Builders<ReportScheduleReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ReportScheduleReadModel>
            .Update.Set(x => x.Name, e.Name)
            .Set(x => x.ReportType, e.ReportType)
            .Set(x => x.Frequency, e.Frequency)
            .Set(x => x.RecipientEmail, e.RecipientEmail)
            .Set(x => x.TimeOfDayUtc, e.TimeOfDayUtc)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.CategoryId, e.CategoryId)
            .Set(x => x.ClientId, e.ClientId)
            .Set(x => x.EmployeeId, e.EmployeeId);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ReportScheduleDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ReportScheduleReadModel>(MongoCollections.ReportSchedules);
        await col.DeleteOneAsync(x => x.Id == $"report_schedule:{e.ReportScheduleId}", ct);
    }

    public async Task ProjectAsync(
        ReportScheduleExecutionSucceededDomainEvent e,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ReportScheduleReadModel>(MongoCollections.ReportSchedules);
        var filter = Builders<ReportScheduleReadModel>.Filter.Eq(
            x => x.Id,
            $"report_schedule:{e.ReportScheduleId}"
        );

        var update = Builders<ReportScheduleReadModel>
            .Update.Set(x => x.LastAttemptAtUtc, e.ExecutedAtUtc)
            .Set(x => x.LastSentAtUtc, e.ExecutedAtUtc)
            .Set(x => x.LastError, null);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(ReportScheduleExecutionFailedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ReportScheduleReadModel>(MongoCollections.ReportSchedules);
        var filter = Builders<ReportScheduleReadModel>.Filter.Eq(
            x => x.Id,
            $"report_schedule:{e.ReportScheduleId}"
        );

        var update = Builders<ReportScheduleReadModel>
            .Update.Set(x => x.LastAttemptAtUtc, e.FailedAtUtc)
            .Set(x => x.LastError, e.ErrorMessage);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
}
