using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Domain.SupplierAttachments;
using Domain.SupplierContacts;
using Domain.Suppliers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Projections.CRM;

internal sealed class SupplierProjection
    : IProjector<SupplierCreatedDomainEvent>,
        IProjector<SupplierUpdatedDomainEvent>,
        IProjector<SupplierDeletedDomainEvent>,
        IProjector<SupplierContactCreatedDomainEvent>,
        IProjector<SupplierContactUpdatedDomainEvent>,
        IProjector<SupplierContactDeletedDomainEvent>,
        IProjector<SupplierPrimaryContactSetDomainEvent>,
        IProjector<SupplierAttachmentUploadedDomainEvent>,
        IProjector<SupplierAttachmentDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public SupplierProjection(IMongoDatabase db) => _db = db;

    // =========================
    // ROOT
    // =========================

    public Task ProjectAsync(SupplierCreatedDomainEvent e, CancellationToken ct) =>
        UpsertRootAsync(
            supplierId: e.SupplierId,
            identificationType: e.IdentificationType,
            identification: e.Identification,
            legalName: e.LegalName,
            tradeName: e.TradeName,
            email: e.Email,
            phone: e.Phone,
            paymentTerms: e.PaymentTerms,
            mainCurrency: e.MainCurrency,
            isActive: e.IsActive,
            createdAt: e.CreatedAt,
            updatedAt: e.UpdatedAt,
            ct
        );

    public Task ProjectAsync(SupplierUpdatedDomainEvent e, CancellationToken ct) =>
        UpdateRootAsync(
            supplierId: e.SupplierId,
            legalName: e.LegalName,
            tradeName: e.TradeName,
            email: e.Email,
            phone: e.Phone,
            paymentTerms: e.PaymentTerms,
            mainCurrency: e.MainCurrency,
            isActive: e.IsActive,
            updatedAt: e.UpdatedAt,
            ct
        );

    public async Task ProjectAsync(SupplierDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);

        var id = $"supplier:{e.SupplierId}";
        await col.DeleteOneAsync(x => x.Id == id, ct);
    }

    // =========================
    // CONTACTS
    // =========================

    public async Task ProjectAsync(SupplierContactCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);
        var id = $"supplier:{e.SupplierId}";

        if (e.IsPrimary)
        {
            var unsetAll = Builders<SupplierReadModel>
                .Update.Set("Contacts.$[].IsPrimary", false)
                .Set(x => x.UpdatedAt, e.UpdatedAt);

            await col.UpdateOneAsync(x => x.Id == id, unsetAll, cancellationToken: ct);
        }

        var contact = new ContactReadModel
        {
            ContactId = e.ContactId,
            Name = e.Name,
            JobTitle = e.JobTitle,
            Email = e.Email,
            Phone = e.Phone!,
            IsPrimary = e.IsPrimary,
        };

        var update = Builders<SupplierReadModel>
            .Update.Push(x => x.Contacts, contact)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }

    public async Task ProjectAsync(SupplierContactUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);
        var id = $"supplier:{e.SupplierId}";

        if (e.IsPrimary)
        {
            var primaryUpdate = Builders<SupplierReadModel>
                .Update.Set("Contacts.$[].IsPrimary", false)
                .Set("Contacts.$[c].IsPrimary", true)
                .Set(x => x.UpdatedAt, e.UpdatedAt);

            var opts = new UpdateOptions
            {
                ArrayFilters =
                [
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                        new BsonDocument("c.ContactId", e.ContactId.ToString()) // Guid match
                    ),
                ],
            };

            await col.UpdateOneAsync(x => x.Id == id, primaryUpdate, opts, ct);
        }

        var update = Builders<SupplierReadModel>
            .Update.Set("Contacts.$[c].Name", e.Name)
            .Set("Contacts.$[c].JobTitle", e.JobTitle)
            .Set("Contacts.$[c].Email", e.Email)
            .Set("Contacts.$[c].Phone", e.Phone)
            .Set("Contacts.$[c].IsPrimary", e.IsPrimary)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        var options = new UpdateOptions
        {
            ArrayFilters =
            [
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("c.ContactId", e.ContactId.ToString()) // Guid match
                ),
            ],
        };

        await col.UpdateOneAsync(x => x.Id == id, update, options, ct);
    }

    public async Task ProjectAsync(SupplierContactDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);
        var id = $"supplier:{e.SupplierId}";

        var update = Builders<SupplierReadModel>
            .Update.PullFilter(x => x.Contacts, c => c.ContactId == e.ContactId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }

    public async Task ProjectAsync(SupplierPrimaryContactSetDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);
        var id = $"supplier:{e.SupplierId}";

        var update = Builders<SupplierReadModel>
            .Update.Set("Contacts.$[].IsPrimary", false)
            .Set("Contacts.$[c].IsPrimary", true)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        var options = new UpdateOptions
        {
            ArrayFilters =
            [
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("c.ContactId", e.ContactId.ToString()) // Guid match
                ),
            ],
        };

        await col.UpdateOneAsync(x => x.Id == id, update, options, ct);
    }

    // =========================
    // ATTACHMENTS
    // =========================

    public async Task ProjectAsync(SupplierAttachmentUploadedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);
        var id = $"supplier:{e.SupplierId}";

        // idempotency (remove if exists, then push)
        var pull = Builders<SupplierReadModel>.Update.PullFilter(
            x => x.Attachments,
            a => a.AttachmentId == e.AttachmentId
        );

        await col.UpdateOneAsync(x => x.Id == id, pull, cancellationToken: ct);

        var attachment = new AttachmentReadModel
        {
            AttachmentId = e.AttachmentId,
            FileName = e.FileName,
            FileS3Key = e.FileS3Key,
            DocumentType = e.DocumentType,
            UploadedAt = e.UploadedAt,
        };

        var push = Builders<SupplierReadModel>
            .Update.Push(x => x.Attachments, attachment)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        await col.UpdateOneAsync(x => x.Id == id, push, cancellationToken: ct);
    }

    public async Task ProjectAsync(SupplierAttachmentDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);
        var id = $"supplier:{e.SupplierId}";

        var update = Builders<SupplierReadModel>
            .Update.PullFilter(x => x.Attachments, a => a.AttachmentId == e.AttachmentId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }

    // =========================
    // Helpers (root upsert/update)
    // =========================

    private async Task UpsertRootAsync(
        Guid supplierId,
        string identificationType,
        string identification,
        string legalName,
        string tradeName,
        string email,
        string phone,
        string paymentTerms,
        string mainCurrency,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);

        var id = $"supplier:{supplierId}";
        var filter = Builders<SupplierReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<SupplierReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.SupplierId, supplierId)
            .Set(x => x.IdentificationType, identificationType)
            .Set(x => x.Identification, identification)
            .Set(x => x.LegalName, legalName)
            .Set(x => x.TradeName, tradeName)
            .Set(x => x.Email, email)
            .Set(x => x.Phone, phone)
            .Set(x => x.PaymentTerms, paymentTerms)
            .Set(x => x.MainCurrency, mainCurrency)
            .Set(x => x.IsActive, isActive)
            .Set(x => x.UpdatedAt, updatedAt)
            .SetOnInsert(x => x.CreatedAt, createdAt)
            .SetOnInsert(x => x.Contacts, new List<ContactReadModel>())
            .SetOnInsert(x => x.Attachments, new List<AttachmentReadModel>());

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task UpdateRootAsync(
        Guid supplierId,
        string legalName,
        string tradeName,
        string email,
        string phone,
        string paymentTerms,
        string mainCurrency,
        bool isActive,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);
        var id = $"supplier:{supplierId}";

        var update = Builders<SupplierReadModel>
            .Update.Set(x => x.LegalName, legalName)
            .Set(x => x.TradeName, tradeName)
            .Set(x => x.Email, email)
            .Set(x => x.Phone, phone)
            .Set(x => x.PaymentTerms, paymentTerms)
            .Set(x => x.MainCurrency, mainCurrency)
            .Set(x => x.IsActive, isActive)
            .Set(x => x.UpdatedAt, updatedAt);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }
}
