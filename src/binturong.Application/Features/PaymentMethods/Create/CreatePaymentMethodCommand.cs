using Application.Abstractions.Messaging;

namespace Application.Features.PaymentMethods.Create;

public sealed record CreatePaymentMethodCommand(string Code, string Description, bool IsActive)
    : ICommand<Guid>;
