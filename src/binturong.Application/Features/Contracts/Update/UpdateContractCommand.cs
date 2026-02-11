using Application.Abstractions.Messaging;

namespace Application.Features.Contracts.Update;

public sealed record UpdateContractCommand(
    Guid ContractId,
    string Code,
    Guid ClientId,
    Guid? QuoteId,
    Guid? SalesOrderId,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status,
    string Description,
    string Notes
) : ICommand;
