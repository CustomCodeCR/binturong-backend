using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.Taxes.GetTaxById;

public sealed record GetTaxByIdQuery(Guid TaxId) : IQuery<TaxReadModel>;
