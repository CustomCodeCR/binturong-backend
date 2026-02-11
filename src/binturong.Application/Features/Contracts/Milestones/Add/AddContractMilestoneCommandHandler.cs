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
        var contract = await _db
            .Contracts.Include(x => x.BillingMilestones)
            .FirstOrDefaultAsync(x => x.Id == cmd.ContractId, ct);

        if (contract is null)
            return Result.Failure<Guid>(ContractErrors.NotFound(cmd.ContractId));

        var desc = cmd.Description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(desc))
            return Result.Failure<Guid>(ContractErrors.MilestoneDescriptionRequired);

        if (cmd.Percentage < 0 || cmd.Percentage > 100)
            return Result.Failure<Guid>(ContractErrors.MilestonePercentageInvalid);

        if (cmd.Amount < 0)
            return Result.Failure<Guid>(ContractErrors.MilestoneAmountInvalid);

        var m = new ContractBillingMilestone
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            Description = desc,
            Percentage = cmd.Percentage,
            Amount = cmd.Amount,
            ScheduledDate = cmd.ScheduledDate,
            IsBilled = false,
            InvoiceId = null,
        };

        contract.AddMilestone(m);

        await _db.SaveChangesAsync(ct);
        return m.Id;
    }
}
