using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.UnitsOfMeasure.Create;

internal sealed class CreateUnitOfMeasureCommandHandler
    : ICommandHandler<CreateUnitOfMeasureCommand, Guid>
{
    private const string Module = "UnitsOfMeasure";
    private const string Entity = "UnitOfMeasure";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateUnitOfMeasureCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateUnitOfMeasureCommand command, CancellationToken ct)
    {
        var code = command.Code.Trim().ToUpperInvariant();
        var name = command.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Guid>(UnitOfMeasureErrors.CodeIsRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Guid>(UnitOfMeasureErrors.NameIsRequired);

        var codeExists = await _db.UnitsOfMeasure.AnyAsync(x => x.Code.ToUpper() == code, ct);
        if (codeExists)
            return Result.Failure<Guid>(UnitOfMeasureErrors.CodeNotUnique);

        var now = DateTime.UtcNow;

        var uom = new UnitOfMeasure
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        uom.RaiseCreated();

        _db.UnitsOfMeasure.Add(uom);
        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId, // Guid? userId
                Module,
                Entity,
                uom.Id, // Guid? entityId
                "UOM_CREATED",
                string.Empty,
                $"uomId={uom.Id}; code={uom.Code}; name={uom.Name}; isActive={uom.IsActive}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success(uom.Id);
    }
}
