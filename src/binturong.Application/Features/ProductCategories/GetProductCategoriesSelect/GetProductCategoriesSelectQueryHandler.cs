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

namespace Application.Features.ProductCategories.GetProductCategoriesSelect;

internal sealed class GetProductCategoriesSelectQueryHandler
    : IQueryHandler<GetProductCategoriesSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Categories";
    private const string Entity = "Category";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetProductCategoriesSelectQueryHandler(
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
        GetProductCategoriesSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ProductCategoryReadModel>(MongoCollections.ProductCategories);

        var filter = BuildFilter(query.Search, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.Name)
            .Project(x => new SelectOptionDto(x.CategoryId.ToString(), $"{x.Name}", x.Name))
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "CATEGORIES_SELECT_READ",
                string.Empty,
                $"search={query.Search ?? ""}; onlyActive={query.OnlyActive}; returned={docs.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(docs);
    }

    private static FilterDefinition<ProductCategoryReadModel> BuildFilter(
        string? search,
        bool onlyActive
    )
    {
        var builder = Builders<ProductCategoryReadModel>.Filter;
        var filters = new List<FilterDefinition<ProductCategoryReadModel>>();

        if (onlyActive)
            filters.Add(builder.Eq(x => x.IsActive, true));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            var re = new BsonRegularExpression(s, "i");

            filters.Add(builder.Or(builder.Regex(x => x.Name, re), builder.Regex(x => x.Name, re)));
        }

        return filters.Count == 0 ? builder.Empty : builder.And(filters);
    }
}
