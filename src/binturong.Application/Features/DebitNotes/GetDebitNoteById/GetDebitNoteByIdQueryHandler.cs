using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.DebitNotes.GetDebitNoteById;

internal sealed class GetDebitNoteByIdQueryHandler
    : IQueryHandler<GetDebitNoteByIdQuery, DebitNoteReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetDebitNoteByIdQueryHandler(
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

    public async Task<Result<DebitNoteReadModel>> Handle(
        GetDebitNoteByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<DebitNoteReadModel>(MongoCollections.DebitNotes);
        var doc = await col.Find(x => x.Id == $"debit_note:{query.Id}").FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "DebitNotes",
            "DebitNote",
            query.Id,
            "DEBIT_NOTE_READ",
            string.Empty,
            doc is null ? "not_found" : "ok",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<DebitNoteReadModel>(
                Error.NotFound("DebitNotes.NotFound", $"DebitNote '{query.Id}' not found.")
            )
            : Result.Success(doc);
    }
}
