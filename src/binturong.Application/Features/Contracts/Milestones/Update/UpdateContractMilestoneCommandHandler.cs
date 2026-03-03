using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Contracts.Milestones.Update;

internal sealed class UpdateContractMilestoneCommandHandler
    : ICommandHandler<UpdateContractMilestoneCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateContractMilestoneCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateContractMilestoneCommand cmd, CancellationToken ct)
    {
        var desc = (cmd.Description ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(desc))
            return Result.Failure(ContractErrors.MilestoneDescriptionRequired);

        if (cmd.Percentage < 0 || cmd.Percentage > 100)
            return Result.Failure(ContractErrors.MilestonePercentageInvalid);

        if (cmd.Amount < 0)
            return Result.Failure(ContractErrors.MilestoneAmountInvalid);

        // Ensure contract exists (fast, avoids loading aggregate + concurrency token issues)
        var contractExists = await _db.Contracts.AnyAsync(x => x.Id == cmd.ContractId, ct);

        if (!contractExists)
            return Result.Failure(ContractErrors.NotFound(cmd.ContractId));

        // Load milestone directly from DbSet (single row tracking)
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

        // Apply changes
        milestone.Description = desc;
        milestone.Percentage = cmd.Percentage;
        milestone.Amount = cmd.Amount;
        milestone.ScheduledDate = cmd.ScheduledDate;
        milestone.IsBilled = cmd.IsBilled;
        milestone.InvoiceId = cmd.InvoiceId;

        try
        {
            await _db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            // If you have ContractErrors.ConcurrencyConflict, reuse it:
            return Result.Failure(ContractErrors.ConcurrencyConflict);
        }
    }
}
