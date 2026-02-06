using Application.Abstractions.Messaging;
using Application.ReadModels.Audit;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Audit.GetAuditLogs;

internal sealed class GetAuditLogsQueryHandler
    : IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogReadModel>>
{
    private readonly IMongoDatabase _db;

    public GetAuditLogsQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<AuditLogReadModel>>> Handle(
        GetAuditLogsQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<AuditLogReadModel>("audit_logs");

        var filter = Builders<AuditLogReadModel>.Filter.Empty;

        if (q.From is not null)
            filter &= Builders<AuditLogReadModel>.Filter.Gte(x => x.EventDate, q.From);

        if (q.To is not null)
            filter &= Builders<AuditLogReadModel>.Filter.Lte(x => x.EventDate, q.To);

        if (q.Module is not null)
            filter &= Builders<AuditLogReadModel>.Filter.Eq(x => x.Module, q.Module);

        if (q.Action is not null)
            filter &= Builders<AuditLogReadModel>.Filter.Eq(x => x.Action, q.Action);

        var list = await col.Find(filter).SortByDescending(x => x.EventDate).ToListAsync(ct);
        return Result.Success<IReadOnlyList<AuditLogReadModel>>(list);
    }
}
