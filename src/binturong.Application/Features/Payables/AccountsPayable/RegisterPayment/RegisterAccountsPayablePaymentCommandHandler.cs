using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.AccountsPayable;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payables.AccountsPayable.RegisterPayment;

internal sealed class RegisterAccountsPayablePaymentCommandHandler
    : ICommandHandler<RegisterAccountsPayablePaymentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RegisterAccountsPayablePaymentCommandHandler(
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
        RegisterAccountsPayablePaymentCommand cmd,
        CancellationToken ct
    )
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (cmd.AccountPayableId == Guid.Empty)
            return Result.Failure(
                Error.Validation("AccountsPayable.IdRequired", "AccountPayableId is required")
            );

        if (cmd.Amount <= 0)
            return Result.Failure(AccountPayableErrors.PaymentAmountInvalid);

        var ap = await _db.AccountsPayable.FirstOrDefaultAsync(
            x => x.Id == cmd.AccountPayableId,
            ct
        );

        if (ap is null)
            return Result.Failure(AccountPayableErrors.NotFound(cmd.AccountPayableId));

        var balanceBefore = ap.PendingBalance;

        // =========================
        // DOMAIN LOGIC (ROOT)
        // =========================
        var result = ap.RegisterPayment(cmd.Amount, cmd.PaidAtUtc, cmd.Notes);

        if (result.IsFailure)
            return result;

        // =========================
        // PERSIST
        // =========================
        await _db.SaveChangesAsync(ct);

        // =========================
        // AUDIT
        // =========================
        await _bus.AuditAsync(
            userId,
            "Payables",
            "AccountsPayable",
            ap.Id,
            "ACCOUNTS_PAYABLE_PAYMENT_REGISTERED",
            string.Empty,
            $"accountPayableId={ap.Id}; "
                + $"amount={cmd.Amount}; "
                + $"balanceBefore={balanceBefore}; "
                + $"balanceAfter={ap.PendingBalance}; "
                + $"status={ap.Status}; "
                + $"paidAt={cmd.PaidAtUtc:o}; "
                + $"notes={cmd.Notes}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
