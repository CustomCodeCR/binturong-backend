using Application.Abstractions.Messaging;

namespace Application.Features.PaymentMethods.Delete;

public sealed record DeletePaymentMethodCommand(Guid PaymentMethodId) : ICommand;
