using System.Text.RegularExpressions;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Common.Selects;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.Payroll;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Employees.GetEmployeesSelect;

internal sealed class GetEmployeesSelectQueryHandler
    : IQueryHandler<GetEmployeesSelectQuery, IReadOnlyList<SelectOptionDto>>
{
    private const string Module = "Employees";
    private const string Entity = "Employee";
    private const int MaxSelectResults = 100;

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetEmployeesSelectQueryHandler(
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
        GetEmployeesSelectQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<EmployeeReadModel>(MongoCollections.Employees);

        var normalizedSearch = Normalize(query.Search);
        var filter = BuildFilter(normalizedSearch, query.OnlyActive);

        var docs = await col.Find(filter)
            .SortBy(x => x.FullName)
            .ThenBy(x => x.NationalId)
            .ThenBy(x => x.Email)
            .Limit(MaxSelectResults)
            .ToListAsync(ct);

        var result = docs.Select(x => new SelectOptionDto(
                x.EmployeeId.ToString(),
                BuildLabel(x),
                BuildCode(x)
            ))
            .ToList();

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "EMPLOYEES_SELECT_READ",
                string.Empty,
                $"search={normalizedSearch ?? ""}; onlyActive={query.OnlyActive}; limit={MaxSelectResults}; returned={result.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<SelectOptionDto>>(result);
    }

    private static FilterDefinition<EmployeeReadModel> BuildFilter(string? search, bool onlyActive)
    {
        var builder = Builders<EmployeeReadModel>.Filter;
        var filters = new List<FilterDefinition<EmployeeReadModel>>();

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
                    builder.Regex(x => x.NationalId, startsWithRegex),
                    builder.Regex(x => x.Email, startsWithRegex),
                    builder.Regex(x => x.FullName, containsRegex),
                    builder.Regex(x => x.JobTitle, containsRegex),
                    builder.Regex(x => x.BranchName, containsRegex)
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

    private static string BuildLabel(EmployeeReadModel x)
    {
        var fullName = x.FullName?.Trim();
        var nationalId = x.NationalId?.Trim();
        var jobTitle = x.JobTitle?.Trim();
        var branchName = x.BranchName?.Trim();

        if (
            !string.IsNullOrWhiteSpace(fullName)
            && !string.IsNullOrWhiteSpace(nationalId)
            && !string.IsNullOrWhiteSpace(branchName)
        )
        {
            return $"{fullName} - {nationalId} ({branchName})";
        }

        if (!string.IsNullOrWhiteSpace(fullName) && !string.IsNullOrWhiteSpace(nationalId))
        {
            return $"{fullName} - {nationalId}";
        }

        if (!string.IsNullOrWhiteSpace(fullName) && !string.IsNullOrWhiteSpace(jobTitle))
        {
            return $"{fullName} - {jobTitle}";
        }

        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName;
        }

        if (!string.IsNullOrWhiteSpace(nationalId))
        {
            return nationalId;
        }

        if (!string.IsNullOrWhiteSpace(x.Email))
        {
            return x.Email;
        }

        return x.EmployeeId.ToString();
    }

    private static string? BuildCode(EmployeeReadModel x)
    {
        if (!string.IsNullOrWhiteSpace(x.NationalId))
            return x.NationalId;

        if (!string.IsNullOrWhiteSpace(x.Email))
            return x.Email;

        return null;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
