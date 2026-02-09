using Application.Abstractions.Messaging;

namespace Application.Features.Payables.AccountsPayable.RegisterPayment;

public sealed record RegisterAccountsPayablePaymentCommand(
    Guid AccountPayableId,
    decimal Amount,
    DateTime PaidAtUtc,
    string? Notes
) : ICommand;
