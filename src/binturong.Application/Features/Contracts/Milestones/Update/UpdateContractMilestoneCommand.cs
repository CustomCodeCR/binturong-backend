using Application.Abstractions.Messaging;

namespace Application.Features.Contracts.Milestones.Update;

public sealed record UpdateContractMilestoneCommand(
    Guid ContractId,
    Guid MilestoneId,
    string Description,
    decimal Percentage,
    decimal Amount,
    DateOnly ScheduledDate,
    bool IsBilled,
    Guid? InvoiceId
) : ICommand;
