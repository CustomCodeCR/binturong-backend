using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.PaymentMethods.GetPaymentMethodById;

public sealed record GetPaymentMethodByIdQuery(Guid PaymentMethodId)
    : IQuery<PaymentMethodReadModel>;
