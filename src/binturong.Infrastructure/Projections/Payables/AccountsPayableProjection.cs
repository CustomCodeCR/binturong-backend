using Application.Abstractions.Projections;
using Application.ReadModels.CRM;
using Application.ReadModels.Payables;
using Domain.AccountsPayable;
using MongoDB.Driver;

namespace Infrastructure.Projections.Payables;

internal sealed class AccountsPayableProjection
    : IProjector<AccountPayableCreatedDomainEvent>,
        IProjector<AccountPayablePaymentRegisteredDomainEvent>
{
    private const string ApCollection = "accounts_payable";
    private const string SuppliersCollection = "suppliers";

    private readonly IMongoDatabase _db;

    public AccountsPayableProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(AccountPayableCreatedDomainEvent e, CancellationToken ct)
    {
        var apCol = _db.GetCollection<AccountsPayableReadModel>(ApCollection);

        var supplierName = await ResolveSupplierName(e.SupplierId, ct);

        var doc = new AccountsPayableReadModel
        {
            Id = $"ap:{e.AccountPayableId}",
            AccountPayableId = e.AccountPayableId,

            SupplierId = e.SupplierId,
            SupplierName = supplierName,

            PurchaseOrderId = e.PurchaseOrderId,
            SupplierInvoiceId = string.IsNullOrWhiteSpace(e.SupplierInvoiceId)
                ? null
                : e.SupplierInvoiceId,

            DocumentDate = e.DocumentDate.ToDateTime(TimeOnly.MinValue),
            DueDate = e.DueDate.ToDateTime(TimeOnly.MinValue),

            TotalAmount = e.TotalAmount,
            PendingBalance = e.PendingBalance,

            Currency = e.Currency,
            Status = e.Status,
        };

        // Use overload with options to avoid obsolete warning
        await apCol.InsertOneAsync(doc, new InsertOneOptions(), ct);
    }

    public async Task ProjectAsync(
        AccountPayablePaymentRegisteredDomainEvent e,
        CancellationToken ct
    )
    {
        var apCol = _db.GetCollection<AccountsPayableReadModel>(ApCollection);

        var update = Builders<AccountsPayableReadModel>
            .Update.Set(x => x.PendingBalance, e.BalanceAfter)
            .Set(x => x.Status, e.StatusAfter);

        await apCol.UpdateOneAsync(
            x => x.AccountPayableId == e.AccountPayableId,
            update,
            cancellationToken: ct
        );
    }

    private async Task<string> ResolveSupplierName(Guid supplierId, CancellationToken ct)
    {
        // Try to load from Mongo suppliers read model
        var supCol = _db.GetCollection<SupplierReadModel>(SuppliersCollection);

        var sup = await supCol.Find(x => x.SupplierId == supplierId).FirstOrDefaultAsync(ct);

        if (sup is null)
            return string.Empty;

        // Prefer TradeName if available, else LegalName
        if (!string.IsNullOrWhiteSpace(sup.TradeName))
            return sup.TradeName;

        return sup.LegalName ?? string.Empty;
    }
}
