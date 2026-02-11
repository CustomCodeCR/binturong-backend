using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Contracts.Update;

internal sealed class UpdateContractCommandHandler : ICommandHandler<UpdateContractCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateContractCommandHandler(IApplicationDbContext db) => _db = db;

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

        contract.Code = cmd.Code.Trim();
        contract.ClientId = cmd.ClientId;
        contract.QuoteId = cmd.QuoteId;
        contract.SalesOrderId = cmd.SalesOrderId;
        contract.StartDate = cmd.StartDate;
        contract.EndDate = cmd.EndDate;
        contract.Status = cmd.Status.Trim();
        contract.Description = cmd.Description?.Trim() ?? string.Empty;
        contract.Notes = cmd.Notes?.Trim() ?? string.Empty;

        contract.RaiseUpdated();

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
