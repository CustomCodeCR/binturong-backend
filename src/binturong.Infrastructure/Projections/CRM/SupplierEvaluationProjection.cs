using Application.Abstractions.Projections;
using Application.ReadModels.CRM;
using Domain.SupplierEvaluations;
using MongoDB.Driver;

namespace Infrastructure.Projections.CRM;

internal sealed class SupplierEvaluationProjection
    : IProjector<SupplierEvaluationCreatedDomainEvent>
{
    private const string EvaluationsCol = "supplier_evaluations";
    private const string SuppliersCol = "suppliers";

    private readonly IMongoDatabase _db;

    public SupplierEvaluationProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(SupplierEvaluationCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierEvaluationReadModel>(EvaluationsCol);

        var supplierName = await ResolveSupplierName(e.SupplierId, ct);

        var doc = new SupplierEvaluationReadModel
        {
            Id = $"supplier_eval:{e.SupplierEvaluationId}",
            SupplierEvaluationId = e.SupplierEvaluationId,
            SupplierId = e.SupplierId,
            SupplierName = supplierName,
            Score = e.Score,
            Classification = e.Classification,
            Comment = e.Comment,
            EvaluatedAtUtc = e.EvaluatedAtUtc,
        };

        await col.InsertOneAsync(doc, new InsertOneOptions(), ct);
    }

    private async Task<string> ResolveSupplierName(Guid supplierId, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierReadModel>(SuppliersCol);
        var s = await col.Find(x => x.SupplierId == supplierId).FirstOrDefaultAsync(ct);
        if (s is null)
            return string.Empty;
        return !string.IsNullOrWhiteSpace(s.TradeName) ? s.TradeName : s.LegalName;
    }
}
