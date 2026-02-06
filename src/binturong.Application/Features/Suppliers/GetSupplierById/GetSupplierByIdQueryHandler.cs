using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Domain.Suppliers;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Suppliers.GetSupplierById;

internal sealed class GetSupplierByIdQueryHandler
    : IQueryHandler<GetSupplierByIdQuery, SupplierReadModel>
{
    private const string Module = "Suppliers";
    private const string Entity = "Supplier";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetSupplierByIdQueryHandler(
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

    public async Task<Result<SupplierReadModel>> Handle(
        GetSupplierByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);
        var id = $"supplier:{query.SupplierId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        if (doc is null)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    _currentUser.UserId, // Guid? userId
                    Module, // string module
                    Entity, // string entity
                    query.SupplierId, // Guid? entityId  ✅ (Guid, no string)
                    "SUPPLIER_READ_NOT_FOUND", // string action
                    string.Empty, // string dataBefore
                    $"supplierId={query.SupplierId}", // string dataAfter
                    _request.IpAddress, // string ip
                    _request.UserAgent // string userAgent
                ),
                ct
            );

            return Result.Failure<SupplierReadModel>(SupplierErrors.NotFound(query.SupplierId));
        }

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                query.SupplierId,
                "SUPPLIER_READ",
                string.Empty,
                $"supplierId={query.SupplierId}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success(doc);
    }
}
