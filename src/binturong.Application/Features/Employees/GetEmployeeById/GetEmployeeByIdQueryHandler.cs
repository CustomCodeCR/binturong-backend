using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Payroll;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Employees.GetEmployeeById;

internal sealed class GetEmployeeByIdQueryHandler
    : IQueryHandler<GetEmployeeByIdQuery, EmployeeReadModel>
{
    private readonly IMongoDatabase _db;

    public GetEmployeeByIdQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<EmployeeReadModel>> Handle(
        GetEmployeeByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<EmployeeReadModel>(MongoCollections.Employees);
        var id = $"employee:{query.EmployeeId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc is null)
            return Result.Failure<EmployeeReadModel>(
                Error.NotFound("Employees.NotFound", $"Employee '{query.EmployeeId}' not found")
            );

        return Result.Success(doc);
    }
}
