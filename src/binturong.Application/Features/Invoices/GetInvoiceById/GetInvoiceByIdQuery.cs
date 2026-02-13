using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.Invoices.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(Guid Id) : IQuery<InvoiceReadModel>;
