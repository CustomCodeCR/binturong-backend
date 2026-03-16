using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Payroll;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Employees.GetEmployees;

internal sealed class GetEmployeesQueryHandler
    : IQueryHandler<GetEmployeesQuery, IReadOnlyList<EmployeeReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetEmployeesQueryHandler(
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

    public async Task<Result<IReadOnlyList<EmployeeReadModel>>> Handle(
        GetEmployeesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<EmployeeReadModel>(MongoCollections.Employees);

        var filters = new List<FilterDefinition<EmployeeReadModel>>();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            var regex = new BsonRegularExpression(s, "i");

            filters.Add(
                Builders<EmployeeReadModel>.Filter.Or(
                    Builders<EmployeeReadModel>.Filter.Regex(x => x.FullName, regex),
                    Builders<EmployeeReadModel>.Filter.Regex(x => x.NationalId, regex),
                    Builders<EmployeeReadModel>.Filter.Regex(x => x.Email, regex),
                    Builders<EmployeeReadModel>.Filter.Regex(x => x.JobTitle, regex),
                    Builders<EmployeeReadModel>.Filter.Regex(x => x.BranchName, regex)
                )
            );
        }

        if (query.UserId.HasValue)
        {
            filters.Add(Builders<EmployeeReadModel>.Filter.Eq(x => x.UserId, query.UserId.Value));
        }

        var filter =
            filters.Count > 0
                ? Builders<EmployeeReadModel>.Filter.And(filters)
                : Builders<EmployeeReadModel>.Filter.Empty;

        var docs = await col.Find(filter).Skip(query.Skip).Limit(query.Take).ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Employees",
            "Employee",
            null,
            "EMPLOYEE_LIST_READ",
            string.Empty,
            $"search={query.Search}; userId={query.UserId}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<EmployeeReadModel>>(docs);
    }
}
