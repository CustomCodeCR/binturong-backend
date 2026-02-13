using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.Payments.GetPaymentById;

public sealed record GetPaymentByIdQuery(Guid Id) : IQuery<PaymentReadModel>;
