using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Payroll;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Payroll.GetPayrolls;

internal sealed class GetPayrollsQueryHandler
    : IQueryHandler<GetPayrollsQuery, IReadOnlyList<PayrollReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPayrollsQueryHandler(
        IMongoDatabase db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<PayrollReadModel>>> Handle(
        GetPayrollsQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PayrollReadModel>(MongoCollections.Payrolls);

        var filter = BuildFilter(query);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.StartDate)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payroll",
            "Payroll",
            null,
            "PAYROLL_LIST_READ",
            string.Empty,
            $"search={query.Search}; from={query.From}; to={query.To}; status={query.Status}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<PayrollReadModel>>(docs);
    }

    private static FilterDefinition<PayrollReadModel> BuildFilter(GetPayrollsQuery q)
    {
        var f = Builders<PayrollReadModel>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            f &= Builders<PayrollReadModel>.Filter.Or(
                Builders<PayrollReadModel>.Filter.Regex(
                    x => x.PeriodCode,
                    new BsonRegularExpression(s, "i")
                ),
                Builders<PayrollReadModel>.Filter.Regex(
                    x => x.PayrollType,
                    new BsonRegularExpression(s, "i")
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(q.Status))
            f &= Builders<PayrollReadModel>.Filter.Eq(x => x.Status, q.Status.Trim());

        if (q.From is not null)
            f &= Builders<PayrollReadModel>.Filter.Gte(x => x.StartDate, q.From.Value);

        if (q.To is not null)
            f &= Builders<PayrollReadModel>.Filter.Lte(x => x.EndDate, q.To.Value);

        return f;
    }
}
