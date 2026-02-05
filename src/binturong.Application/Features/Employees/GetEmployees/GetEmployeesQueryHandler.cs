using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Payroll;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Employees.GetEmployees;

internal sealed class GetEmployeesQueryHandler
    : IQueryHandler<GetEmployeesQuery, IReadOnlyList<EmployeeReadModel>>
{
    private readonly IMongoDatabase _db;

    public GetEmployeesQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<EmployeeReadModel>>> Handle(
        GetEmployeesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<EmployeeReadModel>(MongoCollections.Employees);

        var filter = Builders<EmployeeReadModel>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            filter = Builders<EmployeeReadModel>.Filter.Or(
                Builders<EmployeeReadModel>.Filter.Regex(
                    x => x.FullName,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<EmployeeReadModel>.Filter.Regex(
                    x => x.NationalId,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<EmployeeReadModel>.Filter.Regex(
                    x => x.JobTitle,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                )
            );
        }

        var docs = await col.Find(filter).Skip(query.Skip).Limit(query.Take).ToListAsync(ct);

        return Result.Success<IReadOnlyList<EmployeeReadModel>>(docs);
    }
}
