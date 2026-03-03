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
        // Ensure contract exists (fast, avoids loading aggregate and its concurrency token)
        var contractExists = await _db.Contracts.AnyAsync(x => x.Id == cmd.ContractId, ct);

        if (!contractExists)
            return Result.Failure(ContractErrors.NotFound(cmd.ContractId));

        // Load milestone directly
        var milestone = await _db.ContractBillingMilestones.FirstOrDefaultAsync(
            x => x.Id == cmd.MilestoneId && x.ContractId == cmd.ContractId,
            ct
        );

        if (milestone is null)
            return Result.Failure(
                Error.NotFound(
                    "Contracts.Milestones.NotFound",
                    $"Milestone '{cmd.MilestoneId}' not found."
                )
            );

        _db.ContractBillingMilestones.Remove(milestone);

        try
        {
            await _db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            // In case someone deleted it between load and delete
            return Result.Failure(ContractErrors.ConcurrencyConflict);
        }
    }
}
