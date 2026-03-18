using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Contracts.Update;

internal sealed class UpdateContractCommandHandler : ICommandHandler<UpdateContractCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateContractCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateContractCommand cmd, CancellationToken ct)
    {
        var contract = await _db
            .Contracts.Include(x => x.BillingMilestones)
            .FirstOrDefaultAsync(x => x.Id == cmd.ContractId, ct);

        if (contract is null)
            return Result.Failure(ContractErrors.NotFound(cmd.ContractId));

        if (cmd.ClientId == Guid.Empty)
            return Result.Failure(ContractErrors.ClientRequired);

        if (!await _db.Clients.AnyAsync(x => x.Id == cmd.ClientId, ct))
            return Result.Failure(ContractErrors.ClientNotFound(cmd.ClientId));

        if (cmd.EndDate is not null && cmd.EndDate.Value < cmd.StartDate)
            return Result.Failure(ContractErrors.InvalidValidity(cmd.StartDate, cmd.EndDate.Value));

        if (cmd.ExpiryNoticeDays < 0)
            return Result.Failure(
                Error.Validation(
                    "Contracts.ExpiryNoticeDays.Invalid",
                    "ExpiryNoticeDays must be greater than or equal to zero."
                )
            );

        if (cmd.AutoRenewEnabled && cmd.AutoRenewEveryDays <= 0)
            return Result.Failure(
                Error.Validation(
                    "Contracts.AutoRenewEveryDays.Invalid",
                    "AutoRenewEveryDays must be greater than zero when auto renew is enabled."
                )
            );

        contract.Code = cmd.Code.Trim();
        contract.ClientId = cmd.ClientId;
        contract.QuoteId = cmd.QuoteId;
        contract.SalesOrderId = cmd.SalesOrderId;
        contract.StartDate = cmd.StartDate;
        contract.EndDate = cmd.EndDate;
        contract.Status = cmd.Status.Trim();
        contract.Description = cmd.Description?.Trim() ?? string.Empty;
        contract.Notes = cmd.Notes?.Trim() ?? string.Empty;
        contract.ResponsibleUserId = _currentUser.UserId;
        contract.AutoRenewEnabled = cmd.AutoRenewEnabled;
        contract.AutoRenewEveryDays = cmd.AutoRenewEnabled ? cmd.AutoRenewEveryDays : 365;
        contract.ExpiryNoticeDays = cmd.ExpiryNoticeDays;

        contract.RaiseUpdated();

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
