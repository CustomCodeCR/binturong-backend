using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Payroll;
using Application.ReadModels.Services;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Employees.GetWorkHistory;

internal sealed class GetEmployeeWorkHistoryQueryHandler
    : IQueryHandler<GetEmployeeWorkHistoryQuery, EmployeeWorkHistoryReadModel>
{
    private readonly IMongoDatabase _mongo;

    public GetEmployeeWorkHistoryQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<EmployeeWorkHistoryReadModel>> Handle(
        GetEmployeeWorkHistoryQuery q,
        CancellationToken ct
    )
    {
        var employees = _mongo.GetCollection<EmployeeReadModel>(MongoCollections.Employees);
        var orders = _mongo.GetCollection<ServiceOrderReadModel>(MongoCollections.ServiceOrders);

        var employee = await employees
            .Find(x => x.EmployeeId == q.EmployeeId)
            .FirstOrDefaultAsync(ct);
        if (employee is null)
        {
            return Result.Failure<EmployeeWorkHistoryReadModel>(
                Error.NotFound("Employees.NotFound", $"Employee '{q.EmployeeId}' not found.")
            );
        }

        var docs = await orders
            .Find(x => x.Technicians.Any(t => t.EmployeeId == q.EmployeeId))
            .SortByDescending(x => x.ScheduledDate)
            .ToListAsync(ct);

        var result = new EmployeeWorkHistoryReadModel
        {
            EmployeeId = employee.EmployeeId,
            EmployeeName = employee.FullName,
            Entries = docs.Select(x => new EmployeeWorkHistoryEntryReadModel
                {
                    ServiceOrderId = x.ServiceOrderId,
                    ServiceOrderCode = x.Code,
                    ScheduledDate = x.ScheduledDate,
                    ClosedDate = x.ClosedDate,
                    Status = x.Status,
                    ClientName = x.ClientName,
                    ServiceAddress = x.ServiceAddress,
                    Notes = x.Notes,
                    Services = x.Services.Select(s => s.ServiceName).ToArray(),
                })
                .ToArray(),
        };

        return Result.Success(result);
    }
}
