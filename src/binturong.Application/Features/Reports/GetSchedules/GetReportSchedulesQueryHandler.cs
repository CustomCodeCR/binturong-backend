using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Reports;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Reports.GetSchedules;

internal sealed class GetReportSchedulesQueryHandler
    : IQueryHandler<GetReportSchedulesQuery, IReadOnlyList<ReportScheduleReadModel>>
{
    private readonly IMongoDatabase _mongo;

    public GetReportSchedulesQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<IReadOnlyList<ReportScheduleReadModel>>> Handle(
        GetReportSchedulesQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<ReportScheduleReadModel>(MongoCollections.ReportSchedules);

        var builder = Builders<ReportScheduleReadModel>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            filter &= builder.Or(
                builder.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(s, "i")),
                builder.Regex(x => x.ReportType, new MongoDB.Bson.BsonRegularExpression(s, "i")),
                builder.Regex(x => x.RecipientEmail, new MongoDB.Bson.BsonRegularExpression(s, "i"))
            );
        }

        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);

        var docs = await col.Find(filter)
            .SortBy(x => x.Name)
            .Skip(skip)
            .Limit(Math.Clamp(q.PageSize, 1, 200))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<ReportScheduleReadModel>>(docs);
    }
}
