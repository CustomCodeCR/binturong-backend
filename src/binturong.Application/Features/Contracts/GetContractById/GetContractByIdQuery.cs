using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.Contracts.GetContractById;

public sealed record GetContractByIdQuery(Guid ContractId) : IQuery<ContractReadModel>;
