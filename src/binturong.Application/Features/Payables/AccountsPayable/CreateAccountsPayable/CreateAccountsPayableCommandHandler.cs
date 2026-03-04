using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.AccountsPayable;
using SharedKernel;

namespace Application.Features.Payables.AccountsPayable.CreateAccountsPayable;

internal sealed class CreateAccountsPayableCommandHandler
    : ICommandHandler<CreateAccountsPayableCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateAccountsPayableCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateAccountsPayableCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (cmd.TotalAmount <= 0)
            return Result.Failure<Guid>(AccountPayableErrors.TotalAmountInvalid);

        var ap = new AccountPayable
        {
            Id = Guid.NewGuid(),
            SupplierId = cmd.SupplierId,
            PurchaseOrderId = cmd.PurchaseOrderId,
            SupplierInvoiceId = cmd.SupplierInvoiceId,
            DocumentDate = cmd.DocumentDate,
            DueDate = cmd.DueDate,
            TotalAmount = cmd.TotalAmount,
            PendingBalance = cmd.TotalAmount,
            Currency = cmd.Currency,
            Status = "Pending",
        };

        ap.RaiseCreated();

        _db.AccountsPayable.Add(ap);

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Payables",
            "AccountsPayable",
            ap.Id,
            "ACCOUNTS_PAYABLE_CREATED",
            string.Empty,
            $"supplierId={ap.SupplierId}; total={ap.TotalAmount}; dueDate={ap.DueDate}",
            ip,
            ua,
            ct
        );

        return Result.Success(ap.Id);
    }
}
