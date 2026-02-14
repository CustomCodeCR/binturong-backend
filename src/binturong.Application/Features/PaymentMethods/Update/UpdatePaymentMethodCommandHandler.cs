using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.PaymentMethods.Update;

internal sealed class UpdatePaymentMethodCommandHandler
    : ICommandHandler<UpdatePaymentMethodCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdatePaymentMethodCommandHandler(
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

    public async Task<Result> Handle(UpdatePaymentMethodCommand cmd, CancellationToken ct)
    {
        var pm = await _db.PaymentMethods.FirstOrDefaultAsync(x => x.Id == cmd.PaymentMethodId, ct);
        if (pm is null)
            return Result.Failure(
                Error.NotFound(
                    "PaymentMethods.NotFound",
                    $"PaymentMethod '{cmd.PaymentMethodId}' not found."
                )
            );

        var code = cmd.Code.Trim();
        var desc = cmd.Description.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(
                Error.Validation("PaymentMethods.CodeRequired", "Code is required.")
            );

        if (string.IsNullOrWhiteSpace(desc))
            return Result.Failure(
                Error.Validation("PaymentMethods.DescriptionRequired", "Description is required.")
            );

        var codeTaken = await _db.PaymentMethods.AnyAsync(x => x.Id != pm.Id && x.Code == code, ct);
        if (codeTaken)
            return Result.Failure(
                Error.Conflict("PaymentMethods.CodeExists", $"Payment method code '{code}' exists.")
            );

        pm.Code = code;
        pm.Description = desc;
        pm.IsActive = cmd.IsActive;

        pm.RaiseUpdated();
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "PaymentMethods",
            "PaymentMethod",
            pm.Id,
            "PAYMENT_METHOD_UPDATED",
            string.Empty,
            $"code={pm.Code}; active={pm.IsActive}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
