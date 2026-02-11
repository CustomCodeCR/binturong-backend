using Application.Abstractions.Messaging;

namespace Application.Features.Contracts.Milestones.Remove;

public sealed record RemoveContractMilestoneCommand(Guid ContractId, Guid MilestoneId) : ICommand;
