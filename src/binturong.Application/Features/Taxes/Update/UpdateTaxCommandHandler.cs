using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.Taxes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Taxes.Update;

internal sealed class UpdateTaxCommandHandler : ICommandHandler<UpdateTaxCommand>
{
    private const string Module = "Taxes";
    private const string Entity = "Tax";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateTaxCommandHandler(
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

    public async Task<Result> Handle(UpdateTaxCommand command, CancellationToken ct)
    {
        var tax = await _db.Taxes.FirstOrDefaultAsync(x => x.Id == command.TaxId, ct);
        if (tax is null)
            return Result.Failure(TaxErrors.NotFound(command.TaxId));

        var before =
            $"taxId={tax.Id}; code={tax.Code}; name={tax.Name}; percentage={tax.Percentage}; isActive={tax.IsActive}";

        var name = command.Name.Trim();
        var code = command.Code.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(TaxErrors.NameIsRequired);

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(TaxErrors.CodeIsRequired);

        if (command.Percentage < 0 || command.Percentage > 100)
            return Result.Failure(TaxErrors.InvalidPercentage);

        var codeExists = await _db.Taxes.AnyAsync(
            x => x.Id != command.TaxId && x.Code.ToUpper() == code,
            ct
        );

        if (codeExists)
            return Result.Failure(TaxErrors.CodeNotUnique);

        tax.Name = name;
        tax.Code = code;
        tax.Percentage = command.Percentage;
        tax.IsActive = command.IsActive;
        tax.UpdatedAt = DateTime.UtcNow;

        tax.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        var after =
            $"taxId={tax.Id}; code={tax.Code}; name={tax.Name}; percentage={tax.Percentage}; isActive={tax.IsActive}";

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                command.TaxId,
                "TAX_UPDATED",
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
