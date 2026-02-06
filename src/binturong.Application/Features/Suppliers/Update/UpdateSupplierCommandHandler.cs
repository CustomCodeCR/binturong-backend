using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Update;

internal sealed class UpdateSupplierCommandHandler : ICommandHandler<UpdateSupplierCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    private const string Module = "Suppliers";
    private const string Entity = "Supplier";

    public UpdateSupplierCommandHandler(
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

    public async Task<Result> Handle(UpdateSupplierCommand command, CancellationToken ct)
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
                    "SUPPLIER_UPDATE_FAILED",
                    DataBefore: string.Empty,
                    DataAfter: $"reason=not_found; supplierId={command.SupplierId}",
                    ip,
                    ua
                ),
                ct
            );

            return Result.Failure(SupplierErrors.NotFound(command.SupplierId));
        }

        var email = command.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(command.LegalName))
            return Result.Failure(SupplierErrors.LegalNameIsRequired);

        if (string.IsNullOrWhiteSpace(command.TradeName))
            return Result.Failure(SupplierErrors.TradeNameIsRequired);

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(SupplierErrors.EmailIsRequired);

        if (string.IsNullOrWhiteSpace(command.Phone))
            return Result.Failure(SupplierErrors.PhoneIsRequired);

        var before =
            $"supplierId={supplier.Id}; legalName={supplier.LegalName}; tradeName={supplier.TradeName}; email={supplier.Email}; phone={supplier.Phone}; paymentTerms={supplier.PaymentTerms}; mainCurrency={supplier.MainCurrency}; isActive={supplier.IsActive}";

        var emailExists = await _db.Suppliers.AnyAsync(
            x => x.Id != command.SupplierId && x.Email.ToLower() == email,
            ct
        );

        if (emailExists)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    userId,
                    Module,
                    Entity,
                    supplier.Id,
                    "SUPPLIER_UPDATE_FAILED",
                    before,
                    $"reason=email_not_unique; attemptedEmail={email}",
                    ip,
                    ua
                ),
                ct
            );

            return Result.Failure(SupplierErrors.EmailNotUnique);
        }

        supplier.UpdateBasicInfo(
            legalName: command.LegalName.Trim(),
            tradeName: command.TradeName.Trim(),
            email: email,
            phone: command.Phone.Trim(),
            paymentTerms: string.IsNullOrWhiteSpace(command.PaymentTerms)
                ? null
                : command.PaymentTerms.Trim(),
            mainCurrency: string.IsNullOrWhiteSpace(command.MainCurrency)
                ? null
                : command.MainCurrency.Trim(),
            isActive: command.IsActive
        );

        await _db.SaveChangesAsync(ct);

        var after =
            $"supplierId={supplier.Id}; legalName={supplier.LegalName}; tradeName={supplier.TradeName}; email={supplier.Email}; phone={supplier.Phone}; paymentTerms={supplier.PaymentTerms}; mainCurrency={supplier.MainCurrency}; isActive={supplier.IsActive}";

        await _bus.Send(
            new CreateAuditLogCommand(
                userId,
                Module,
                Entity,
                supplier.Id,
                "SUPPLIER_UPDATED",
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
