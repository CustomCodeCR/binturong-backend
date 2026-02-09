using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.SupplierEvaluations;
using SharedKernel;

namespace Application.Features.SupplierEvaluations.Create;

internal sealed class CreateSupplierEvaluationCommandHandler
    : ICommandHandler<CreateSupplierEvaluationCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateSupplierEvaluationCommandHandler(
        IApplicationDbContext db,
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

    public async Task<Result<Guid>> Handle(
        CreateSupplierEvaluationCommand cmd,
        CancellationToken ct
    )
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var created = SupplierEvaluation.Create(
            cmd.SupplierId,
            cmd.Score,
            cmd.Comment,
            cmd.EvaluatedAtUtc
        );
        if (created.IsFailure)
            return Result.Failure<Guid>(created.Error);

        var eval = created.Value;

        _db.SupplierEvaluations.Add(eval);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Suppliers",
            "SupplierEvaluation",
            eval.Id,
            "SUPPLIER_EVALUATION_CREATED",
            string.Empty,
            $"supplierEvaluationId={eval.Id}; supplierId={eval.SupplierId}; score={eval.Score}; classification={eval.Classification}; evaluatedAt={eval.EvaluatedAtUtc:o}",
            ip,
            ua,
            ct
        );

        return Result.Success(eval.Id);
    }
}
