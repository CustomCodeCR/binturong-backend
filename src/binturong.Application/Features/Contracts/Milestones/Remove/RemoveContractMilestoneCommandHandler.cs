using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Contracts.Milestones.Remove;

internal sealed class RemoveContractMilestoneCommandHandler
    : ICommandHandler<RemoveContractMilestoneCommand>
{
    private readonly IApplicationDbContext _db;

    public RemoveContractMilestoneCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(RemoveContractMilestoneCommand cmd, CancellationToken ct)
    {
        var contract = await _db
            .Contracts.Include(x => x.BillingMilestones)
            .FirstOrDefaultAsync(x => x.Id == cmd.ContractId, ct);

        if (contract is null)
            return Result.Failure(ContractErrors.NotFound(cmd.ContractId));

        var exists = contract.BillingMilestones.Any(x => x.Id == cmd.MilestoneId);
        if (!exists)
            return Result.Failure(
                Error.NotFound(
                    "Contracts.Milestones.NotFound",
                    $"Milestone '{cmd.MilestoneId}' not found."
                )
            );

        contract.RemoveMilestone(cmd.MilestoneId);

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
