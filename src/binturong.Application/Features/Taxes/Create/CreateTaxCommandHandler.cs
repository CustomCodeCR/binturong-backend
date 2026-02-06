using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.Taxes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Taxes.Create;

internal sealed class CreateTaxCommandHandler : ICommandHandler<CreateTaxCommand, Guid>
{
    private const string Module = "Taxes";
    private const string Entity = "Tax";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateTaxCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateTaxCommand command, CancellationToken ct)
    {
        var name = command.Name.Trim();
        var code = command.Code.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Guid>(TaxErrors.NameIsRequired);

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Guid>(TaxErrors.CodeIsRequired);

        if (command.Percentage < 0 || command.Percentage > 100)
            return Result.Failure<Guid>(TaxErrors.InvalidPercentage);

        var codeExists = await _db.Taxes.AnyAsync(x => x.Code.ToUpper() == code, ct);
        if (codeExists)
            return Result.Failure<Guid>(TaxErrors.CodeNotUnique);

        var now = DateTime.UtcNow;

        var tax = new Tax
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Percentage = command.Percentage,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        tax.RaiseCreated();

        _db.Taxes.Add(tax);
        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId, // Guid? userId
                Module, // string module
                Entity, // string entity
                tax.Id, // Guid? entityId
                "TAX_CREATED", // string action
                string.Empty, // dataBefore
                $"taxId={tax.Id}; code={tax.Code}; name={tax.Name}; percentage={tax.Percentage}; isActive={tax.IsActive}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success(tax.Id);
    }
}
