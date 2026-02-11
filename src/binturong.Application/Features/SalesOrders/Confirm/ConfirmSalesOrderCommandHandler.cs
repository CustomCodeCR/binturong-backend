using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.SalesOrders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.SalesOrders.Confirm;

internal sealed class ConfirmSalesOrderCommandHandler : ICommandHandler<ConfirmSalesOrderCommand>
{
    private readonly IApplicationDbContext _db;

    public ConfirmSalesOrderCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(ConfirmSalesOrderCommand cmd, CancellationToken ct)
    {
        if (cmd.SellerUserId == Guid.Empty)
            return Result.Failure(SalesOrderErrors.SellerRequiredForCommission);

        var so = await _db.SalesOrders.FirstOrDefaultAsync(x => x.Id == cmd.SalesOrderId, ct);
        if (so is null)
            return Result.Failure(SalesOrderErrors.NotFound(cmd.SalesOrderId));

        if (string.Equals(so.Status, "Confirmed", StringComparison.OrdinalIgnoreCase))
            return Result.Failure(SalesOrderErrors.AlreadyConfirmed);

        so.Status = "Confirmed";

        var now = DateTime.UtcNow;
        so.RaiseConfirmed(cmd.SellerUserId);

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
