using System.Text.RegularExpressions;
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

namespace Application.Features.Branches.GetBranchesSelect;

internal sealed class GetBranchesSelectQueryHandler
    : IQueryHandler<GetBranchesSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Branches";
    private const string Entity = "Branch";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetBranchesSelectQueryHandler(
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
        GetBranchesSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.Code)
            .ThenBy(x => x.Name)
            .Limit(MaxSelectResults)
            .Project(x => new SelectOptionDto(x.BranchId.ToString(), BuildLabel(x), x.Code))
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "BRANCHES_SELECT_READ",
                string.Empty,
                $"search={normalizedSearch ?? ""}; onlyActive={query.OnlyActive}; limit={MaxSelectResults}; returned={docs.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(docs);
    }

    private static FilterDefinition<BranchReadModel> BuildFilter(string? search, bool onlyActive)
    {
        var builder = Builders<BranchReadModel>.Filter;
        var filters = new List<FilterDefinition<BranchReadModel>>();

        if (onlyActive)
        {
            filters.Add(builder.Eq(x => x.IsActive, true));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escaped = Regex.Escape(search);
            var startsWithRegex = new BsonRegularExpression($"^{escaped}", "i");
            var containsRegex = new BsonRegularExpression(escaped, "i");

            filters.Add(
                builder.Or(
                    builder.Regex(x => x.Code, startsWithRegex),
                    builder.Regex(x => x.Phone, startsWithRegex),
                    builder.Regex(x => x.Name, containsRegex),
                    builder.Regex(x => x.Address, containsRegex)
                )
            );
        }

        return filters.Count switch
        {
            0 => builder.Empty,
            1 => filters[0],
            _ => builder.And(filters),
        };
    }

    private static string BuildLabel(BranchReadModel x)
    {
        var code = x.Code?.Trim();
        var name = x.Name?.Trim();

        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
            return $"{code} - {name}";

        if (!string.IsNullOrWhiteSpace(name))
            return name;

        if (!string.IsNullOrWhiteSpace(code))
            return code;

        return x.BranchId.ToString();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
