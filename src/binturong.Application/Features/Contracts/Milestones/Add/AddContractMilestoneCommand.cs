using Application.Abstractions.Messaging;

namespace Application.Features.Contracts.Milestones.Add;

public sealed record AddContractMilestoneCommand(
    Guid ContractId,
    string Description,
    decimal Percentage,
    decimal Amount,
    DateOnly ScheduledDate
) : ICommand<Guid>;
