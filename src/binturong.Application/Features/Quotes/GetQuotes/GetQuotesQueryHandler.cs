using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Sales;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Quotes.GetQuotes;

internal sealed class GetQuotesQueryHandler
    : IQueryHandler<GetQuotesQuery, IReadOnlyList<QuoteReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetQuotesQueryHandler(
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

    public async Task<Result<IReadOnlyList<QuoteReadModel>>> Handle(
        GetQuotesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<QuoteReadModel>("quotes");

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.IssueDate)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Sales",
            "Quote",
            null,
            "QUOTE_LIST_READ",
            string.Empty,
            $"search={query.Search}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<QuoteReadModel>>(docs);
    }

    private static FilterDefinition<QuoteReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<QuoteReadModel>.Filter.Empty;

        var s = search.Trim();

        // Búsqueda flexible: code, client name, branch name, status
        return Builders<QuoteReadModel>.Filter.Or(
            Builders<QuoteReadModel>.Filter.Regex(x => x.Code, new BsonRegularExpression(s, "i")),
            Builders<QuoteReadModel>.Filter.Regex(
                x => x.ClientName,
                new BsonRegularExpression(s, "i")
            ),
            Builders<QuoteReadModel>.Filter.Regex(
                x => x.BranchName,
                new BsonRegularExpression(s, "i")
            ),
            Builders<QuoteReadModel>.Filter.Regex(x => x.Status, new BsonRegularExpression(s, "i"))
        );
    }
}
