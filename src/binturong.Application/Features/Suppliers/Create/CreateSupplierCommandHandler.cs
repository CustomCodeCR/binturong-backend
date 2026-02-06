using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Create;

internal sealed class CreateSupplierCommandHandler : ICommandHandler<CreateSupplierCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    private const string Module = "Suppliers";
    private const string Entity = "Supplier";

    public CreateSupplierCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateSupplierCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        var identification = command.Identification.Trim();
        var email = command.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(identification))
            return Result.Failure<Guid>(SupplierErrors.IdentificationIsRequired);

        if (string.IsNullOrWhiteSpace(command.LegalName))
            return Result.Failure<Guid>(SupplierErrors.LegalNameIsRequired);

        if (string.IsNullOrWhiteSpace(command.TradeName))
            return Result.Failure<Guid>(SupplierErrors.TradeNameIsRequired);

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Guid>(SupplierErrors.EmailIsRequired);

        if (string.IsNullOrWhiteSpace(command.Phone))
            return Result.Failure<Guid>(SupplierErrors.PhoneIsRequired);

        // Uniqueness checks
        var emailExists = await _db.Suppliers.AnyAsync(x => x.Email.ToLower() == email, ct);
        if (emailExists)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    userId,
                    Module,
                    Entity,
                    EntityId: null,
                    Action: "SUPPLIER_CREATE_FAILED",
                    DataBefore: string.Empty,
                    DataAfter: $"reason=email_not_unique; email={email}",
                    ip,
                    ua
                ),
                ct
            );

            return Result.Failure<Guid>(SupplierErrors.EmailNotUnique);
        }

        var identificationExists = await _db.Suppliers.AnyAsync(
            x => x.Identification == identification,
            ct
        );
        if (identificationExists)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    userId,
                    Module,
                    Entity,
                    EntityId: null,
                    Action: "SUPPLIER_CREATE_FAILED",
                    DataBefore: string.Empty,
                    DataAfter: $"reason=identification_not_unique; identification={identification}",
                    ip,
                    ua
                ),
                ct
            );

            return Result.Failure<Guid>(SupplierErrors.IdentificationNotUnique);
        }

        var now = DateTime.UtcNow;

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            IdentificationType = command.IdentificationType.Trim(),
            Identification = identification,
            LegalName = command.LegalName.Trim(),
            TradeName = command.TradeName.Trim(),
            Email = email,
            Phone = command.Phone.Trim(),
            PaymentTerms = command.PaymentTerms?.Trim() ?? string.Empty,
            MainCurrency = command.MainCurrency?.Trim() ?? string.Empty,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        supplier.RaiseCreated();

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                userId,
                Module,
                Entity,
                supplier.Id,
                "SUPPLIER_CREATED",
                DataBefore: string.Empty,
                DataAfter: $"supplierId={supplier.Id}; tradeName={supplier.TradeName}; legalName={supplier.LegalName}; email={supplier.Email}; isActive={supplier.IsActive}",
                ip,
                ua
            ),
            ct
        );

        return Result.Success(supplier.Id);
    }
}
