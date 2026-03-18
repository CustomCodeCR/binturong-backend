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

    public CreateContractCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<Guid>> Handle(CreateContractCommand cmd, CancellationToken ct)
    {
        if (cmd.ClientId == Guid.Empty)
            return Result.Failure<Guid>(ContractErrors.ClientRequired);

        if (!await _db.Clients.AnyAsync(x => x.Id == cmd.ClientId, ct))
            return Result.Failure<Guid>(ContractErrors.ClientNotFound(cmd.ClientId));

        if (cmd.ResponsibleUserId == Guid.Empty)
            return Result.Failure<Guid>(
                Error.Validation(
                    "Contracts.ResponsibleUser.Required",
                    "ResponsibleUserId is required."
                )
            );

        if (cmd.EndDate is not null && cmd.EndDate.Value < cmd.StartDate)
            return Result.Failure<Guid>(
                ContractErrors.InvalidValidity(cmd.StartDate, cmd.EndDate.Value)
            );

        if (cmd.ExpiryNoticeDays < 0)
            return Result.Failure<Guid>(
                Error.Validation(
                    "Contracts.ExpiryNoticeDays.Invalid",
                    "ExpiryNoticeDays must be greater than or equal to zero."
                )
            );

        if (cmd.AutoRenewEnabled && cmd.AutoRenewEveryDays <= 0)
            return Result.Failure<Guid>(
                Error.Validation(
                    "Contracts.AutoRenewEveryDays.Invalid",
                    "AutoRenewEveryDays must be greater than zero when auto renew is enabled."
                )
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
            ResponsibleUserId = cmd.ResponsibleUserId,
            AutoRenewEnabled = cmd.AutoRenewEnabled,
            AutoRenewEveryDays = cmd.AutoRenewEnabled ? cmd.AutoRenewEveryDays : 365,
            ExpiryNoticeDays = cmd.ExpiryNoticeDays,
            ExpiryAlertActive = false,
            ExpiryLastNotifiedAtUtc = null,
            RenewedAtUtc = null,
        };

        foreach (var milestoneInput in cmd.Milestones ?? [])
        {
            var description = milestoneInput.Description?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(description))
                return Result.Failure<Guid>(ContractErrors.MilestoneDescriptionRequired);

            if (milestoneInput.Percentage < 0 || milestoneInput.Percentage > 100)
                return Result.Failure<Guid>(ContractErrors.MilestonePercentageInvalid);

            if (milestoneInput.Amount < 0)
                return Result.Failure<Guid>(ContractErrors.MilestoneAmountInvalid);

            var milestone = new ContractBillingMilestone
            {
                Id = Guid.NewGuid(),
                ContractId = contract.Id,
                Description = description,
                Percentage = milestoneInput.Percentage,
                Amount = milestoneInput.Amount,
                ScheduledDate = milestoneInput.ScheduledDate,
                IsBilled = false,
                InvoiceId = null,
            };

            contract.AddMilestone(milestone);
        }

        contract.RaiseCreated();

        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync(ct);

        return contract.Id;
    }
}
