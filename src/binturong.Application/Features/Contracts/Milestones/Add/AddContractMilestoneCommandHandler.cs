using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ContractBillingMilestones;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Contracts.Milestones.Add;

internal sealed class AddContractMilestoneCommandHandler
    : ICommandHandler<AddContractMilestoneCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public AddContractMilestoneCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(AddContractMilestoneCommand cmd, CancellationToken ct)
    {
        // 1) Validate input
        var desc = (cmd.Description ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(desc))
            return Result.Failure<Guid>(ContractErrors.MilestoneDescriptionRequired);

        if (cmd.Percentage < 0 || cmd.Percentage > 100)
            return Result.Failure<Guid>(ContractErrors.MilestonePercentageInvalid);

        if (cmd.Amount < 0)
            return Result.Failure<Guid>(ContractErrors.MilestoneAmountInvalid);

        // 2) Ensure contract exists (NO Include, NO tracking changes needed)
        var contractExists = await _db.Contracts.AnyAsync(x => x.Id == cmd.ContractId, ct);

        if (!contractExists)
            return Result.Failure<Guid>(ContractErrors.NotFound(cmd.ContractId));

        // 3) Optional duplicate check in DB (safe under concurrency)
        var duplicate = await _db.ContractBillingMilestones.AnyAsync(
            x =>
                x.ContractId == cmd.ContractId
                && x.Description == desc
                && x.Percentage == cmd.Percentage
                && x.Amount == cmd.Amount
                && x.ScheduledDate == cmd.ScheduledDate,
            ct
        );

        if (duplicate)
            return Result.Failure<Guid>(ContractErrors.MilestoneDuplicate);

        // 4) Insert milestone directly
        var m = new ContractBillingMilestone
        {
            Id = Guid.NewGuid(),
            ContractId = cmd.ContractId,
            Description = desc,
            Percentage = cmd.Percentage,
            Amount = cmd.Amount,
            ScheduledDate = cmd.ScheduledDate,
            IsBilled = false,
            InvoiceId = null,
        };

        _db.ContractBillingMilestones.Add(m);

        // 5) Save
        await _db.SaveChangesAsync(ct);

        return m.Id;
    }
}
