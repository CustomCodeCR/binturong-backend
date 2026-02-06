using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.Taxes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Taxes.Delete;

internal sealed class DeleteTaxCommandHandler : ICommandHandler<DeleteTaxCommand>
{
    private const string Module = "Taxes";
    private const string Entity = "Tax";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteTaxCommandHandler(
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

    public async Task<Result> Handle(DeleteTaxCommand command, CancellationToken ct)
    {
        var tax = await _db
            .Taxes.Include(x => x.Products) // validate in use
            .FirstOrDefaultAsync(x => x.Id == command.TaxId, ct);

        if (tax is null)
            return Result.Failure(TaxErrors.NotFound(command.TaxId));

        if (tax.Products.Count > 0)
            return Result.Failure(TaxErrors.CannotDeleteInUse);

        var before =
            $"taxId={tax.Id}; code={tax.Code}; name={tax.Name}; percentage={tax.Percentage}; isActive={tax.IsActive}";

        tax.RaiseDeleted();

        _db.Taxes.Remove(tax);
        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                command.TaxId,
                "TAX_DELETED",
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
