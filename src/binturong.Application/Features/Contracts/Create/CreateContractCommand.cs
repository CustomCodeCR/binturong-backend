using Application.Abstractions.Messaging;

namespace Application.Features.Contracts.Create;

public sealed record CreateContractMilestone(
    string Description,
    decimal Percentage,
    decimal Amount,
    DateOnly ScheduledDate
);

public sealed record CreateContractCommand(
    string Code,
    Guid ClientId,
    Guid? QuoteId,
    Guid? SalesOrderId,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status,
    string Description,
    string Notes,
    IReadOnlyList<CreateContractMilestone> Milestones
) : ICommand<Guid>;
