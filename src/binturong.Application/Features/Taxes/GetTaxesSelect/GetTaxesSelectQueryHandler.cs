using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Common.Selects;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Taxes.GetTaxesSelect;

internal sealed class GetTaxesSelectQueryHandler
    : IQueryHandler<GetTaxesSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Taxes";
    private const string Entity = "Tax";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetTaxesSelectQueryHandler(
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

    public async Task<Result<IReadOnlyList<SelectOptionDto>>> Handle(
        GetTaxesSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<TaxReadModel>(MongoCollections.Taxes);

        var filter = BuildFilter(query.Search, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.Code)
            .Project(x => new SelectOptionDto(x.TaxId.ToString(), $"{x.Code} - {x.Name}", x.Code))
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "TAXES_SELECT_READ",
                string.Empty,
                $"search={query.Search ?? ""}; onlyActive={query.OnlyActive}; returned={docs.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(docs);
    }

    private static FilterDefinition<TaxReadModel> BuildFilter(string? search, bool onlyActive)
    {
        var builder = Builders<TaxReadModel>.Filter;
        var filters = new List<FilterDefinition<TaxReadModel>>();

        if (onlyActive)
            filters.Add(builder.Eq(x => x.IsActive, true));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            var re = new BsonRegularExpression(s, "i");

            filters.Add(builder.Or(builder.Regex(x => x.Code, re), builder.Regex(x => x.Name, re)));
        }

        return filters.Count == 0 ? builder.Empty : builder.And(filters);
    }
}
