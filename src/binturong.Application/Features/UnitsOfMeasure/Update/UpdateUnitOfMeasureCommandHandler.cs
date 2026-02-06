using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.UnitsOfMeasure.Update;

internal sealed class UpdateUnitOfMeasureCommandHandler
    : ICommandHandler<UpdateUnitOfMeasureCommand>
{
    private const string Module = "UnitsOfMeasure";
    private const string Entity = "UnitOfMeasure";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateUnitOfMeasureCommandHandler(
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

    public async Task<Result> Handle(UpdateUnitOfMeasureCommand command, CancellationToken ct)
    {
        var uom = await _db.UnitsOfMeasure.FirstOrDefaultAsync(x => x.Id == command.UomId, ct);
        if (uom is null)
            return Result.Failure(UnitOfMeasureErrors.NotFound(command.UomId));

        var before = $"uomId={uom.Id}; code={uom.Code}; name={uom.Name}; isActive={uom.IsActive}";

        var code = command.Code.Trim().ToUpperInvariant();
        var name = command.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(UnitOfMeasureErrors.CodeIsRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(UnitOfMeasureErrors.NameIsRequired);

        var codeExists = await _db.UnitsOfMeasure.AnyAsync(
            x => x.Id != command.UomId && x.Code.ToUpper() == code,
            ct
        );

        if (codeExists)
            return Result.Failure(UnitOfMeasureErrors.CodeNotUnique);

        uom.Code = code;
        uom.Name = name;
        uom.IsActive = command.IsActive;
        uom.UpdatedAt = DateTime.UtcNow;

        uom.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        var after = $"uomId={uom.Id}; code={uom.Code}; name={uom.Name}; isActive={uom.IsActive}";

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                command.UomId,
                "UOM_UPDATED",
                before,
                after,
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success();
    }
}
