using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Payroll;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Employees.GetEmployeeByUserId;

internal sealed class GetEmployeeByUserIdQueryHandler
    : IQueryHandler<GetEmployeeByUserIdQuery, EmployeeReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetEmployeeByUserIdQueryHandler(
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

    public async Task<Result<EmployeeReadModel>> Handle(
        GetEmployeeByUserIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<EmployeeReadModel>(MongoCollections.Employees);

        var doc = await col.Find(x => x.UserId == query.UserId).FirstOrDefaultAsync(ct);

        if (doc is null)
        {
            return Result.Failure<EmployeeReadModel>(
                Error.NotFound(
                    "Employees.NotFoundByUserId",
                    $"Employee with userId '{query.UserId}' not found"
                )
            );
        }

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Employees",
            "Employee",
            doc.EmployeeId,
            "EMPLOYEE_READ_BY_USER_ID",
            string.Empty,
            $"userId={query.UserId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(doc);
    }
}
