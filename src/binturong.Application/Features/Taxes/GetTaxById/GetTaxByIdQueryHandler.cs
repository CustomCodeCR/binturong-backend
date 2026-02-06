using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using Domain.Taxes;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Taxes.GetTaxById;

internal sealed class GetTaxByIdQueryHandler : IQueryHandler<GetTaxByIdQuery, TaxReadModel>
{
    private const string Module = "Taxes";
    private const string Entity = "Tax";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetTaxByIdQueryHandler(
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

    public async Task<Result<TaxReadModel>> Handle(GetTaxByIdQuery query, CancellationToken ct)
    {
        var col = _db.GetCollection<TaxReadModel>(MongoCollections.Taxes);
        var id = $"tax:{query.TaxId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        if (doc is null)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    _currentUser.UserId,
                    Module,
                    Entity,
                    query.TaxId,
                    "TAX_READ_NOT_FOUND",
                    string.Empty,
                    $"taxId={query.TaxId}",
                    _request.IpAddress,
                    _request.UserAgent
                ),
                ct
            );

            return Result.Failure<TaxReadModel>(TaxErrors.NotFound(query.TaxId));
        }

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                query.TaxId,
                "TAX_READ",
                string.Empty,
                $"taxId={query.TaxId}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success(doc);
    }
}
