using Application.Abstractions.Messaging;

namespace Application.Features.Contracts.Delete;

public sealed record DeleteContractCommand(Guid ContractId) : ICommand;
