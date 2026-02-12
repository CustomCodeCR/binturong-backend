using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Payroll;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Payroll.GetPayrollById;

internal sealed class GetPayrollByIdQueryHandler
    : IQueryHandler<GetPayrollByIdQuery, PayrollReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPayrollByIdQueryHandler(
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

    public async Task<Result<PayrollReadModel>> Handle(
        GetPayrollByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PayrollReadModel>(MongoCollections.Payrolls);

        var id = $"payroll:{query.PayrollId}";
        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payroll",
            "Payroll",
            query.PayrollId,
            doc is null ? "PAYROLL_READ_NOT_FOUND" : "PAYROLL_READ",
            string.Empty,
            $"payrollId={query.PayrollId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        if (doc is null)
            return Result.Failure<PayrollReadModel>(
                Error.NotFound("Payroll.NotFound", $"Payroll '{query.PayrollId}' not found.")
            );

        return Result.Success(doc);
    }
}
