using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ContractBillingMilestones;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Contracts.Create;

internal sealed class CreateContractCommandHandler : ICommandHandler<CreateContractCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateContractCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateContractCommand cmd, CancellationToken ct)
    {
        if (cmd.ClientId == Guid.Empty)
            return Result.Failure<Guid>(ContractErrors.ClientRequired);

        if (!await _db.Clients.AnyAsync(x => x.Id == cmd.ClientId, ct))
            return Result.Failure<Guid>(ContractErrors.ClientNotFound(cmd.ClientId));

        if (cmd.EndDate is not null && cmd.EndDate.Value < cmd.StartDate)
            return Result.Failure<Guid>(
                ContractErrors.InvalidValidity(cmd.StartDate, cmd.EndDate.Value)
            );

        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            Code = cmd.Code.Trim(),
            ClientId = cmd.ClientId,
            QuoteId = cmd.QuoteId,
            SalesOrderId = cmd.SalesOrderId,
            StartDate = cmd.StartDate,
            EndDate = cmd.EndDate,
            Status = cmd.Status.Trim(),
            Description = cmd.Description?.Trim() ?? string.Empty,
            Notes = cmd.Notes?.Trim() ?? string.Empty,
        };

        if (cmd.Milestones is not null)
        {
            foreach (var m in cmd.Milestones)
            {
                var desc = m.Description?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(desc))
                    return Result.Failure<Guid>(ContractErrors.MilestoneDescriptionRequired);

                if (m.Percentage < 0 || m.Percentage > 100)
                    return Result.Failure<Guid>(ContractErrors.MilestonePercentageInvalid);

                if (m.Amount < 0)
                    return Result.Failure<Guid>(ContractErrors.MilestoneAmountInvalid);

                var milestone = new ContractBillingMilestone
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    Description = desc,
                    Percentage = m.Percentage,
                    Amount = m.Amount,
                    ScheduledDate = m.ScheduledDate,
                    IsBilled = false,
                    InvoiceId = null,
                };

                contract.AddMilestone(milestone);
            }
        }

        contract.RaiseCreated();

        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync(ct);

        return contract.Id;
    }
}
