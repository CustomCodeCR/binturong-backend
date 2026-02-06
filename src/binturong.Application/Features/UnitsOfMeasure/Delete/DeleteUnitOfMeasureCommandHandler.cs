using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.UnitsOfMeasure.Delete;

internal sealed class DeleteUnitOfMeasureCommandHandler
    : ICommandHandler<DeleteUnitOfMeasureCommand>
{
    private const string Module = "UnitsOfMeasure";
    private const string Entity = "UnitOfMeasure";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteUnitOfMeasureCommandHandler(
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

    public async Task<Result> Handle(DeleteUnitOfMeasureCommand command, CancellationToken ct)
    {
        var uom = await _db
            .UnitsOfMeasure.Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == command.UomId, ct);

        if (uom is null)
            return Result.Failure(UnitOfMeasureErrors.NotFound(command.UomId));

        if (uom.Products.Count > 0)
            return Result.Failure(UnitOfMeasureErrors.CannotDeleteInUse);

        var before = $"uomId={uom.Id}; code={uom.Code}; name={uom.Name}; isActive={uom.IsActive}";

        uom.RaiseDeleted();

        _db.UnitsOfMeasure.Remove(uom);
        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                command.UomId,
                "UOM_DELETED",
                before,
                string.Empty,
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success();
    }
}
