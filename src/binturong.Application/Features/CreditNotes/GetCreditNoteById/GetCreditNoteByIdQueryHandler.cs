using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.CreditNotes.GetCreditNoteById;

internal sealed class GetCreditNoteByIdQueryHandler
    : IQueryHandler<GetCreditNoteByIdQuery, CreditNoteReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetCreditNoteByIdQueryHandler(
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

    public async Task<Result<CreditNoteReadModel>> Handle(
        GetCreditNoteByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<CreditNoteReadModel>(MongoCollections.CreditNotes);
        var doc = await col.Find(x => x.Id == $"credit_note:{query.Id}").FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "CreditNotes",
            "CreditNote",
            query.Id,
            "CREDIT_NOTE_READ",
            string.Empty,
            doc is null ? "not_found" : "ok",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<CreditNoteReadModel>(
                Error.NotFound("CreditNotes.NotFound", $"CreditNote '{query.Id}' not found.")
            )
            : Result.Success(doc);
    }
}
