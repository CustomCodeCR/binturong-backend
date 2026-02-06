using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Payroll;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Employees.GetEmployeeById;

internal sealed class GetEmployeeByIdQueryHandler
    : IQueryHandler<GetEmployeeByIdQuery, EmployeeReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetEmployeeByIdQueryHandler(
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

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Employees",
            "Employee",
            query.EmployeeId,
            "EMPLOYEE_READ",
            string.Empty,
            $"employeeId={query.EmployeeId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(doc);
    }
}
