using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Delete;

internal sealed class DeleteSupplierCommandHandler : ICommandHandler<DeleteSupplierCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    private const string Module = "Suppliers";
    private const string Entity = "Supplier";

    public DeleteSupplierCommandHandler(
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

    public async Task<Result> Handle(DeleteSupplierCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        var supplier = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == command.SupplierId, ct);
        if (supplier is null)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    userId,
                    Module,
                    Entity,
                    command.SupplierId,
                    "SUPPLIER_DELETE_FAILED",
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
            $"supplierId={supplier.Id}; tradeName={supplier.TradeName}; legalName={supplier.LegalName}; email={supplier.Email}; isActive={supplier.IsActive}";

        supplier.RaiseDeleted();

        _db.Suppliers.Remove(supplier);
        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                userId,
                Module,
                Entity,
                supplier.Id,
                "SUPPLIER_DELETED",
                before,
                $"supplierId={supplier.Id}",
                ip,
                ua
            ),
            ct
        );

        return Result.Success();
    }
}
