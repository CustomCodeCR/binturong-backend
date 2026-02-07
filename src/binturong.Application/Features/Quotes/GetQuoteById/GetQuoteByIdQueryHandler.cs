using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Quotes.GetQuoteById;

internal sealed class GetQuoteByIdQueryHandler : IQueryHandler<GetQuoteByIdQuery, QuoteReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetQuoteByIdQueryHandler(
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

    public async Task<Result<QuoteReadModel>> Handle(GetQuoteByIdQuery query, CancellationToken ct)
    {
        var col = _db.GetCollection<QuoteReadModel>("quotes");

        var doc = await col.Find(x => x.QuoteId == query.QuoteId).FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Sales",
            "Quote",
            query.QuoteId,
            "QUOTE_READ",
            string.Empty,
            $"quoteId={query.QuoteId}; found={(doc is not null)}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<QuoteReadModel>(
                Error.NotFound("Quotes.NotFound", $"Quote '{query.QuoteId}' not found")
            )
            : Result.Success(doc);
    }
}
