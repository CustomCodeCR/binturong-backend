using Application.Abstractions.Projections;
using Application.ReadModels.Audit;
using Domain.AuditLogs;
using MongoDB.Driver;

namespace Infrastructure.Projections.Audit;

internal sealed class AuditLogProjection : IProjector<AuditLogCreatedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public AuditLogProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(AuditLogCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<AuditLogReadModel>("audit_logs");

        var rm = new AuditLogReadModel
        {
            Id = $"audit:{e.AuditId}",
            AuditId = e.AuditId.GetHashCode(), // solo para UI legacy
            EventDate = e.EventDate,
            UserId = e.UserId?.GetHashCode(),
            Module = e.Module,
            Entity = e.Entity,
            EntityId = e.EntityId?.GetHashCode(),
            Action = e.Action,
            DataBefore = e.DataBefore,
            DataAfter = e.DataAfter,
            Ip = e.Ip,
            UserAgent = e.UserAgent,
        };

        await col.InsertOneAsync(rm, cancellationToken: ct);
    }
}
