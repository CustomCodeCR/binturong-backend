using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.PaymentMethods.Delete;

internal sealed class DeletePaymentMethodCommandHandler
    : ICommandHandler<DeletePaymentMethodCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeletePaymentMethodCommandHandler(
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

    public async Task<Result> Handle(DeletePaymentMethodCommand cmd, CancellationToken ct)
    {
        var pm = await _db.PaymentMethods.FirstOrDefaultAsync(x => x.Id == cmd.PaymentMethodId, ct);
        if (pm is null)
            return Result.Failure(
                Error.NotFound(
                    "PaymentMethods.NotFound",
                    $"PaymentMethod '{cmd.PaymentMethodId}' not found."
                )
            );

        _db.PaymentMethods.Remove(pm);
        pm.RaiseDeleted();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "PaymentMethods",
            "PaymentMethod",
            pm.Id,
            "PAYMENT_METHOD_DELETED",
            string.Empty,
            $"code={pm.Code}; active={pm.IsActive}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
