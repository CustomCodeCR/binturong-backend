using Application.Abstractions.Messaging;

namespace Application.Features.PaymentMethods.Update;

public sealed record UpdatePaymentMethodCommand(
    Guid PaymentMethodId,
    string Code,
    string Description,
    bool IsActive
) : ICommand;
