using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.CRM;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.SupplierEvaluations.GetSupplierEvaluations;

internal sealed class GetSupplierEvaluationsQueryHandler
    : IQueryHandler<GetSupplierEvaluationsQuery, IReadOnlyList<SupplierEvaluationReadModel>>
{
    private const string CollectionName = "supplier_evaluations";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetSupplierEvaluationsQueryHandler(
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

    public async Task<Result<IReadOnlyList<SupplierEvaluationReadModel>>> Handle(
        GetSupplierEvaluationsQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SupplierEvaluationReadModel>(CollectionName);

        var docs = await col.Find(x => x.SupplierId == q.SupplierId)
            .SortByDescending(x => x.EvaluatedAtUtc)
            .Skip(q.Skip)
            .Limit(q.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Suppliers",
            "SupplierEvaluation",
            null,
            "SUPPLIER_EVALUATION_HISTORY_READ",
            string.Empty,
            $"supplierId={q.SupplierId}; skip={q.Skip}; take={q.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<SupplierEvaluationReadModel>>(docs);
    }
}
