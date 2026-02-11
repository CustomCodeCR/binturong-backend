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
        var contract = await _db
            .Contracts.Include(x => x.BillingMilestones)
            .FirstOrDefaultAsync(x => x.Id == cmd.ContractId, ct);

        if (contract is null)
            return Result.Failure(ContractErrors.NotFound(cmd.ContractId));

        var m = contract.BillingMilestones.FirstOrDefault(x => x.Id == cmd.MilestoneId);
        if (m is null)
            return Result.Failure(
                Error.NotFound(
                    "Contracts.Milestones.NotFound",
                    $"Milestone '{cmd.MilestoneId}' not found."
                )
            );

        var desc = cmd.Description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(desc))
            return Result.Failure(ContractErrors.MilestoneDescriptionRequired);

        if (cmd.Percentage < 0 || cmd.Percentage > 100)
            return Result.Failure(ContractErrors.MilestonePercentageInvalid);

        if (cmd.Amount < 0)
            return Result.Failure(ContractErrors.MilestoneAmountInvalid);

        m.Description = desc;
        m.Percentage = cmd.Percentage;
        m.Amount = cmd.Amount;
        m.ScheduledDate = cmd.ScheduledDate;
        m.IsBilled = cmd.IsBilled;
        m.InvoiceId = cmd.InvoiceId;

        contract.UpdateMilestone(m);

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
