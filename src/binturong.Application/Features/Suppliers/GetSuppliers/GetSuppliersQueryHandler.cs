using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Suppliers.GetSuppliers;

internal sealed class GetSuppliersQueryHandler
    : IQueryHandler<GetSuppliersQuery, IReadOnlyList<SupplierReadModel>>
{
    private const string Module = "Suppliers";
    private const string Entity = "Supplier";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetSuppliersQueryHandler(
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

    public async Task<Result<IReadOnlyList<SupplierReadModel>>> Handle(
        GetSuppliersQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.UpdatedAt)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId, // Guid? userId
                Module,
                Entity,
                null, // Guid? entityId (list)
                "SUPPLIERS_READ",
                string.Empty,
                $"search={query.Search ?? ""}; skip={query.Skip}; take={query.Take}; returned={docs.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SupplierReadModel>>(docs);
    }

    private static FilterDefinition<SupplierReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<SupplierReadModel>.Filter.Empty;

        var s = search.Trim();
        var re = new BsonRegularExpression(s, "i");

        return Builders<SupplierReadModel>.Filter.Or(
            Builders<SupplierReadModel>.Filter.Regex(x => x.TradeName, re),
            Builders<SupplierReadModel>.Filter.Regex(x => x.LegalName, re),
            Builders<SupplierReadModel>.Filter.Regex(x => x.Email, re),
            Builders<SupplierReadModel>.Filter.Regex(x => x.Identification, re),
            Builders<SupplierReadModel>.Filter.Regex(x => x.Phone, re)
        );
    }
}
