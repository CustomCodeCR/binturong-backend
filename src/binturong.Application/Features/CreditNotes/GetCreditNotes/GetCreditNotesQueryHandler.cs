using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.CreditNotes.GetCreditNotes;

internal sealed class GetCreditNotesQueryHandler
    : IQueryHandler<GetCreditNotesQuery, IReadOnlyList<CreditNoteReadModel>>
{
    private readonly IMongoDatabase _mongo;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetCreditNotesQueryHandler(
        IMongoDatabase mongo,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _mongo = mongo;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<CreditNoteReadModel>>> Handle(
        GetCreditNotesQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<CreditNoteReadModel>(MongoCollections.CreditNotes);

        var filter = Builders<CreditNoteReadModel>.Filter.Empty;
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            filter = Builders<CreditNoteReadModel>.Filter.Or(
                Builders<CreditNoteReadModel>.Filter.Regex(
                    x => x.Consecutive,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<CreditNoteReadModel>.Filter.Regex(
                    x => x.Reason,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<CreditNoteReadModel>.Filter.Regex(
                    x => x.InvoiceConsecutive,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                )
            );
        }

        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.IssueDate)
            .Skip(skip)
            .Limit(Math.Clamp(q.PageSize, 1, 200))
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "CreditNotes",
            "CreditNote",
            null,
            "CREDIT_NOTES_LIST",
            string.Empty,
            $"page={q.Page}; pageSize={q.PageSize}; search={(q.Search ?? "")}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<CreditNoteReadModel>>(docs);
    }
}
