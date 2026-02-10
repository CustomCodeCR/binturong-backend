using Application.Abstractions.Projections;
using Application.ReadModels.CRM;
using Application.ReadModels.MasterData;
using Application.ReadModels.Purchases;
using Domain.SupplierQuotes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Purchases;

internal sealed class SupplierQuoteProjection
    : IProjector<SupplierQuoteSentDomainEvent>,
        IProjector<SupplierQuoteLineAddedDomainEvent>,
        IProjector<SupplierQuoteRespondedDomainEvent>,
        IProjector<SupplierQuoteResponseLineRegisteredDomainEvent>,
        IProjector<SupplierQuoteRejectedDomainEvent>
{
    private const string QuotesCol = "supplier_quotes";
    private const string SuppliersCol = "suppliers";
    private const string BranchesCol = "branches";
    private const string ProductsCol = "products";

    private readonly IMongoDatabase _db;

    public SupplierQuoteProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(SupplierQuoteSentDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierQuoteReadModel>(QuotesCol);

        var supplierName = await ResolveSupplierName(e.SupplierId, ct);
        var branchName = e.BranchId.HasValue ? await ResolveBranchName(e.BranchId.Value, ct) : null;

        var doc = new SupplierQuoteReadModel
        {
            Id = $"supplier_quote:{e.SupplierQuoteId}",
            SupplierQuoteId = e.SupplierQuoteId,
            Code = e.Code,
            SupplierId = e.SupplierId,
            SupplierName = supplierName,
            BranchId = e.BranchId,
            BranchName = branchName,
            RequestedAtUtc = e.RequestedAtUtc,
            RespondedAtUtc = null,
            Status = e.Status,
            Notes = e.Notes,
            SupplierMessage = null,
            RejectReason = null,
            Lines = new List<SupplierQuoteLineReadModel>(),
            ResponseLines = new List<SupplierQuoteResponseLineReadModel>(),
        };

        // Idempotent: Replace + upsert avoids duplicate key issues
        await col.ReplaceOneAsync(
            x => x.SupplierQuoteId == e.SupplierQuoteId,
            doc,
            new ReplaceOptions { IsUpsert = true },
            ct
        );
    }

    public async Task ProjectAsync(SupplierQuoteLineAddedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierQuoteReadModel>(QuotesCol);

        var productName = await ResolveProductName(e.ProductId, ct);

        var line = new SupplierQuoteLineReadModel
        {
            SupplierQuoteLineId = e.SupplierQuoteLineId,
            ProductId = e.ProductId,
            ProductName = productName,
            Quantity = e.Quantity,
        };

        // IMPORTANT:
        // Filter by Id (string) which is the natural Mongo document key in your model.
        var filter = Builders<SupplierQuoteReadModel>.Filter.Eq(
            x => x.Id,
            $"supplier_quote:{e.SupplierQuoteId}"
        );

        // IMPORTANT:
        // Upsert so even if events are out-of-order, the doc gets created and the line is pushed.
        var update = Builders<SupplierQuoteReadModel>
            .Update.SetOnInsert(x => x.Id, $"supplier_quote:{e.SupplierQuoteId}")
            .SetOnInsert(x => x.SupplierQuoteId, e.SupplierQuoteId)
            .SetOnInsert(x => x.Code, string.Empty)
            .SetOnInsert(x => x.SupplierId, Guid.Empty)
            .SetOnInsert(x => x.SupplierName, string.Empty)
            .SetOnInsert(x => x.Status, "Sent")
            .SetOnInsert(x => x.RequestedAtUtc, DateTime.UnixEpoch)
            .Push(x => x.Lines, line);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(SupplierQuoteRespondedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierQuoteReadModel>(QuotesCol);

        var filter = Builders<SupplierQuoteReadModel>.Filter.Eq(
            x => x.Id,
            $"supplier_quote:{e.SupplierQuoteId}"
        );

        var update = Builders<SupplierQuoteReadModel>
            .Update.SetOnInsert(x => x.Id, $"supplier_quote:{e.SupplierQuoteId}")
            .SetOnInsert(x => x.SupplierQuoteId, e.SupplierQuoteId)
            .Set(x => x.Status, e.Status)
            .Set(x => x.RespondedAtUtc, e.RespondedAtUtc)
            .Set(x => x.SupplierMessage, e.SupplierMessage);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(
        SupplierQuoteResponseLineRegisteredDomainEvent e,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SupplierQuoteReadModel>(QuotesCol);

        var productName = await ResolveProductName(e.ProductId, ct);

        var responseLine = new SupplierQuoteResponseLineReadModel
        {
            ProductId = e.ProductId,
            ProductName = productName,
            UnitPrice = e.UnitPrice,
            DiscountPerc = e.DiscountPerc,
            TaxPerc = e.TaxPerc,
            Conditions = e.Conditions,
        };

        var filter = Builders<SupplierQuoteReadModel>.Filter.Eq(
            x => x.Id,
            $"supplier_quote:{e.SupplierQuoteId}"
        );

        var update = Builders<SupplierQuoteReadModel>
            .Update.SetOnInsert(x => x.Id, $"supplier_quote:{e.SupplierQuoteId}")
            .SetOnInsert(x => x.SupplierQuoteId, e.SupplierQuoteId)
            .Push(x => x.ResponseLines, responseLine);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(SupplierQuoteRejectedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierQuoteReadModel>(QuotesCol);

        var filter = Builders<SupplierQuoteReadModel>.Filter.Eq(
            x => x.Id,
            $"supplier_quote:{e.SupplierQuoteId}"
        );

        var update = Builders<SupplierQuoteReadModel>
            .Update.SetOnInsert(x => x.Id, $"supplier_quote:{e.SupplierQuoteId}")
            .SetOnInsert(x => x.SupplierQuoteId, e.SupplierQuoteId)
            .Set(x => x.Status, e.Status)
            .Set(x => x.RejectReason, e.Reason);

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task<string> ResolveSupplierName(Guid supplierId, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierReadModel>(SuppliersCol);
        var s = await col.Find(x => x.SupplierId == supplierId).FirstOrDefaultAsync(ct);
        if (s is null)
            return string.Empty;

        return !string.IsNullOrWhiteSpace(s.TradeName) ? s.TradeName : s.LegalName;
    }

    private async Task<string?> ResolveBranchName(Guid branchId, CancellationToken ct)
    {
        var col = _db.GetCollection<BranchReadModel>(BranchesCol);
        var b = await col.Find(x => x.BranchId == branchId).FirstOrDefaultAsync(ct);
        return b?.Name;
    }

    private async Task<string> ResolveProductName(Guid productId, CancellationToken ct)
    {
        var col = _db.GetCollection<Application.ReadModels.Inventory.ProductReadModel>(ProductsCol);
        var p = await col.Find(x => x.ProductId == productId).FirstOrDefaultAsync(ct);
        return p?.Name ?? string.Empty;
    }
}
