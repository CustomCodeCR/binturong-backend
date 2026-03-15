using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.PaymentMethods.GetPaymentMethodsSelect;

public sealed record GetPaymentMethodsSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
