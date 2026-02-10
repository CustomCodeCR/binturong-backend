using Application.Abstractions.Messaging;
using Application.ReadModels.Purchases;

namespace Application.Features.SupplierQuotes.GetSupplierQuoteById;

public sealed record GetSupplierQuoteByIdQuery(Guid SupplierQuoteId)
    : IQuery<SupplierQuoteReadModel>;
