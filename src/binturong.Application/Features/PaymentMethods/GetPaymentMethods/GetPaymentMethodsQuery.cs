using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.PaymentMethods.GetPaymentMethods;

public sealed record GetPaymentMethodsQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<PaymentMethodReadModel>>;
