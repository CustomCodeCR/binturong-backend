using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.SetCreditConditions;

internal sealed class SetSupplierCreditConditionsCommandHandler
    : ICommandHandler<SetSupplierCreditConditionsCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    private const string Module = "Suppliers";
    private const string Entity = "Supplier";

    public SetSupplierCreditConditionsCommandHandler(
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

    public async Task<Result> Handle(
        SetSupplierCreditConditionsCommand command,
        CancellationToken ct
    )
    {
        var userId = _currentUser.UserId;
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        if (!command.HasPermission)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    userId,
                    Module,
                    Entity,
                    command.SupplierId,
                    "SUPPLIER_CREDIT_SET_FAILED",
                    DataBefore: string.Empty,
                    DataAfter: $"reason=unauthorized; supplierId={command.SupplierId}",
                    ip,
                    ua
                ),
                ct
            );

            return Result.Failure(SupplierCreditErrors.Unauthorized);
        }

        var supplier = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == command.SupplierId, ct);
        if (supplier is null)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    userId,
                    Module,
                    Entity,
                    command.SupplierId,
                    "SUPPLIER_CREDIT_SET_FAILED",
                    DataBefore: string.Empty,
                    DataAfter: $"reason=not_found; supplierId={command.SupplierId}",
                    ip,
                    ua
                ),
                ct
            );

            return Result.Failure(SupplierErrors.NotFound(command.SupplierId));
        }

        var before =
            $"supplierId={supplier.Id}; creditLimit={supplier.CreditLimit}; creditDays={supplier.CreditDays}";

        if (command.CreditLimit > 50_000_000)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    userId,
                    Module,
                    Entity,
                    supplier.Id,
                    "SUPPLIER_CREDIT_SET_FAILED",
                    before,
                    $"reason=credit_exceeded; requestedLimit={command.CreditLimit}; requestedDays={command.CreditDays}",
                    ip,
                    ua
                ),
                ct
            );

            return Result.Failure(SupplierCreditErrors.CreditExceeded);
        }

        supplier.SetCreditConditions(command.CreditLimit, command.CreditDays);

        await _db.SaveChangesAsync(ct);

        var after =
            $"supplierId={supplier.Id}; creditLimit={supplier.CreditLimit}; creditDays={supplier.CreditDays}";

        await _bus.Send(
            new CreateAuditLogCommand(
                userId,
                Module,
                Entity,
                supplier.Id,
                "SUPPLIER_CREDIT_SET",
                before,
                after,
                ip,
                ua
            ),
            ct
        );

        return Result.Success();
    }
}
