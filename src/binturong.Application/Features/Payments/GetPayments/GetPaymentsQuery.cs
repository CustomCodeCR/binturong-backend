using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.Payments.GetPayments;

public sealed record GetPaymentsQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null,
    Guid? PaymentMethodId = null
) : IQuery<IReadOnlyList<PaymentReadModel>>;
