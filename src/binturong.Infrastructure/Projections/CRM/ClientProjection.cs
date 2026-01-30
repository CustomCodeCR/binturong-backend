using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Domain.ClientAddresses;
using Domain.ClientAttachments;
using Domain.ClientContacts;
using Domain.Clients;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Projections.CRM;

internal sealed class ClientProjection
    : IProjector<ClientCreatedDomainEvent>,
        IProjector<ClientUpdatedDomainEvent>,
        IProjector<ClientDeletedDomainEvent>,
        IProjector<ClientAddressCreatedDomainEvent>,
        IProjector<ClientAddressUpdatedDomainEvent>,
        IProjector<ClientAddressDeletedDomainEvent>,
        IProjector<ClientPrimaryAddressSetDomainEvent>,
        IProjector<ClientContactCreatedDomainEvent>,
        IProjector<ClientContactUpdatedDomainEvent>,
        IProjector<ClientContactDeletedDomainEvent>,
        IProjector<ClientPrimaryContactSetDomainEvent>,
        IProjector<ClientAttachmentUploadedDomainEvent>,
        IProjector<ClientAttachmentDeletedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public ClientProjection(IMongoDatabase db) => _db = db;

    // =========================
    // ROOT
    // =========================

    public Task ProjectAsync(ClientCreatedDomainEvent e, CancellationToken ct) =>
        UpsertRootAsync(
            clientId: e.ClientId,
            personType: e.PersonType,
            identificationType: e.IdentificationType,
            identification: e.Identification,
            tradeName: e.TradeName,
            contactName: e.ContactName,
            email: e.Email,
            primaryPhone: e.PrimaryPhone,
            secondaryPhone: e.SecondaryPhone,
            industry: e.Industry,
            clientType: e.ClientType,
            score: e.Score,
            isActive: e.IsActive,
            createdAt: e.CreatedAt,
            updatedAt: e.UpdatedAt,
            ct
        );

    public Task ProjectAsync(ClientUpdatedDomainEvent e, CancellationToken ct) =>
        UpdateRootAsync(
            clientId: e.ClientId,
            tradeName: e.TradeName,
            contactName: e.ContactName,
            email: e.Email,
            primaryPhone: e.PrimaryPhone,
            secondaryPhone: e.SecondaryPhone,
            industry: e.Industry,
            clientType: e.ClientType,
            score: e.Score,
            isActive: e.IsActive,
            updatedAt: e.UpdatedAt,
            ct
        );

    public async Task ProjectAsync(ClientDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);

        var id = $"client:{e.ClientId}";
        await col.DeleteOneAsync(x => x.Id == id, ct);
    }

    // =========================
    // ADDRESSES
    // =========================

    public async Task ProjectAsync(ClientAddressCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{e.ClientId}";

        // If new address is primary, unset all others first
        if (e.IsPrimary)
        {
            var unsetAll = Builders<ClientReadModel>
                .Update.Set("Addresses.$[].IsPrimary", false)
                .Set(x => x.UpdatedAt, e.UpdatedAt);

            await col.UpdateOneAsync(x => x.Id == id, unsetAll, cancellationToken: ct);
        }

        var address = new AddressReadModel
        {
            AddressId = e.AddressId,
            AddressType = e.AddressType,
            AddressLine = e.AddressLine,
            Province = e.Province,
            Canton = e.Canton,
            District = e.District,
            Notes = e.Notes,
            IsPrimary = e.IsPrimary,
        };

        var update = Builders<ClientReadModel>
            .Update.Push(x => x.Addresses, address)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }

    public async Task ProjectAsync(ClientAddressUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{e.ClientId}";

        // If update says it's primary, enforce only-one-primary
        if (e.IsPrimary)
        {
            var primaryUpdate = Builders<ClientReadModel>
                .Update.Set("Addresses.$[].IsPrimary", false)
                .Set("Addresses.$[a].IsPrimary", true)
                .Set(x => x.UpdatedAt, e.UpdatedAt);

            var opts = new UpdateOptions
            {
                ArrayFilters =
                [
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                        new BsonDocument("a.AddressId", e.AddressId.ToString())
                    ),
                ],
            };

            await col.UpdateOneAsync(x => x.Id == id, primaryUpdate, opts, ct);
        }

        var update = Builders<ClientReadModel>
            .Update.Set("Addresses.$[a].AddressType", e.AddressType)
            .Set("Addresses.$[a].AddressLine", e.AddressLine)
            .Set("Addresses.$[a].Province", e.Province)
            .Set("Addresses.$[a].Canton", e.Canton)
            .Set("Addresses.$[a].District", e.District)
            .Set("Addresses.$[a].Notes", e.Notes)
            .Set("Addresses.$[a].IsPrimary", e.IsPrimary)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        var options = new UpdateOptions
        {
            ArrayFilters =
            [
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("a.AddressId", e.AddressId.ToString())
                ),
            ],
        };

        await col.UpdateOneAsync(x => x.Id == id, update, options, ct);
    }

    public async Task ProjectAsync(ClientAddressDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{e.ClientId}";

        var update = Builders<ClientReadModel>
            .Update.PullFilter(x => x.Addresses, a => a.AddressId == e.AddressId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }

    public async Task ProjectAsync(ClientPrimaryAddressSetDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{e.ClientId}";

        var update = Builders<ClientReadModel>
            .Update.Set("Addresses.$[].IsPrimary", false)
            .Set("Addresses.$[a].IsPrimary", true)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        var options = new UpdateOptions
        {
            ArrayFilters =
            [
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("a.AddressId", e.AddressId.ToString())
                ),
            ],
        };

        await col.UpdateOneAsync(x => x.Id == id, update, options, ct);
    }

    // =========================
    // CONTACTS
    // =========================

    public async Task ProjectAsync(ClientContactCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{e.ClientId}";

        if (e.IsPrimary)
        {
            var unsetAll = Builders<ClientReadModel>
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

        var update = Builders<ClientReadModel>
            .Update.Push(x => x.Contacts, contact)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }

    public async Task ProjectAsync(ClientContactUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{e.ClientId}";

        if (e.IsPrimary)
        {
            var primaryUpdate = Builders<ClientReadModel>
                .Update.Set("Contacts.$[].IsPrimary", false)
                .Set("Contacts.$[c].IsPrimary", true)
                .Set(x => x.UpdatedAt, e.UpdatedAt);

            var opts = new UpdateOptions
            {
                ArrayFilters =
                [
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                        new BsonDocument("c.ContactId", e.ContactId.ToString())
                    ),
                ],
            };

            await col.UpdateOneAsync(x => x.Id == id, primaryUpdate, opts, ct);
        }

        var update = Builders<ClientReadModel>
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
                    new BsonDocument("c.ContactId", e.ContactId.ToString())
                ),
            ],
        };

        await col.UpdateOneAsync(x => x.Id == id, update, options, ct);
    }

    public async Task ProjectAsync(ClientContactDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{e.ClientId}";

        var update = Builders<ClientReadModel>
            .Update.PullFilter(x => x.Contacts, c => c.ContactId == e.ContactId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }

    public async Task ProjectAsync(ClientPrimaryContactSetDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{e.ClientId}";

        var update = Builders<ClientReadModel>
            .Update.Set("Contacts.$[].IsPrimary", false)
            .Set("Contacts.$[c].IsPrimary", true)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        var options = new UpdateOptions
        {
            ArrayFilters =
            [
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("c.ContactId", e.ContactId.ToString())
                ),
            ],
        };

        await col.UpdateOneAsync(x => x.Id == id, update, options, ct);
    }

    // =========================
    // ATTACHMENTS
    // =========================

    public async Task ProjectAsync(ClientAttachmentUploadedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{e.ClientId}";

        // ensure idempotency (remove if exists, then push)
        var pull = Builders<ClientReadModel>.Update.PullFilter(
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

        var push = Builders<ClientReadModel>
            .Update.Push(x => x.Attachments, attachment)
            .Set(x => x.UpdatedAt, e.UpdatedAt);

        await col.UpdateOneAsync(x => x.Id == id, push, cancellationToken: ct);
    }

    public async Task ProjectAsync(ClientAttachmentDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{e.ClientId}";

        var update = Builders<ClientReadModel>
            .Update.PullFilter(x => x.Attachments, a => a.AttachmentId == e.AttachmentId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }

    // =========================
    // Helpers (root upsert/update)
    // =========================

    private async Task UpsertRootAsync(
        Guid clientId,
        string personType,
        string identificationType,
        string identification,
        string tradeName,
        string contactName,
        string email,
        string primaryPhone,
        string? secondaryPhone,
        string? industry,
        string? clientType,
        int score,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);

        var id = $"client:{clientId}";
        var filter = Builders<ClientReadModel>.Filter.Eq(x => x.Id, id);

        var update = Builders<ClientReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ClientId, clientId)
            .Set(x => x.PersonType, personType)
            .Set(x => x.IdentificationType, identificationType)
            .Set(x => x.Identification, identification)
            .Set(x => x.TradeName, tradeName)
            .Set(x => x.ContactName, contactName)
            .Set(x => x.Email, email)
            .Set(x => x.PrimaryPhone, primaryPhone)
            .Set(x => x.SecondaryPhone, secondaryPhone)
            .Set(x => x.Industry, industry)
            .Set(x => x.ClientType, clientType)
            .Set(x => x.Score, score)
            .Set(x => x.IsActive, isActive)
            .Set(x => x.UpdatedAt, updatedAt)
            .SetOnInsert(x => x.CreatedAt, createdAt)
            // initialize collections + KPIs for new docs
            .SetOnInsert(x => x.Addresses, new List<AddressReadModel>())
            .SetOnInsert(x => x.Contacts, new List<ContactReadModel>())
            .SetOnInsert(x => x.Attachments, new List<AttachmentReadModel>())
            .SetOnInsert(x => x.Kpis, new ClientKpisReadModel());

        await col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    private async Task UpdateRootAsync(
        Guid clientId,
        string tradeName,
        string contactName,
        string email,
        string primaryPhone,
        string? secondaryPhone,
        string? industry,
        string? clientType,
        int score,
        bool isActive,
        DateTime updatedAt,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);
        var id = $"client:{clientId}";

        var update = Builders<ClientReadModel>
            .Update.Set(x => x.TradeName, tradeName)
            .Set(x => x.ContactName, contactName)
            .Set(x => x.Email, email)
            .Set(x => x.PrimaryPhone, primaryPhone)
            .Set(x => x.SecondaryPhone, secondaryPhone)
            .Set(x => x.Industry, industry)
            .Set(x => x.ClientType, clientType)
            .Set(x => x.Score, score)
            .Set(x => x.IsActive, isActive)
            .Set(x => x.UpdatedAt, updatedAt);

        await col.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }
}
