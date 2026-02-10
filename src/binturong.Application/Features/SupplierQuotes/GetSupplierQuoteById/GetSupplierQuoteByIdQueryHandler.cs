using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Purchases;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.SupplierQuotes.GetSupplierQuoteById;

internal sealed class GetSupplierQuoteByIdQueryHandler
    : IQueryHandler<GetSupplierQuoteByIdQuery, SupplierQuoteReadModel>
{
    private const string CollectionName = "supplier_quotes";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetSupplierQuoteByIdQueryHandler(
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

    public async Task<Result<SupplierQuoteReadModel>> Handle(
        GetSupplierQuoteByIdQuery q,
        CancellationToken ct
    )
    {
        if (q.SupplierQuoteId == Guid.Empty)
            return Result.Failure<SupplierQuoteReadModel>(
                Error.Validation("SupplierQuotes.IdRequired", "SupplierQuoteId is required")
            );

        var col = _db.GetCollection<SupplierQuoteReadModel>(CollectionName);

        var doc = await col.Find(x => x.SupplierQuoteId == q.SupplierQuoteId)
            .FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Suppliers",
            "SupplierQuote",
            q.SupplierQuoteId,
            "SUPPLIER_QUOTE_READ",
            string.Empty,
            $"supplierQuoteId={q.SupplierQuoteId}; found={(doc is not null)}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<SupplierQuoteReadModel>(
                Error.NotFound(
                    "SupplierQuotes.NotFound",
                    $"Supplier quote '{q.SupplierQuoteId}' not found"
                )
            )
            : Result.Success(doc);
    }
}
