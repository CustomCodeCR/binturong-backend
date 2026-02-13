using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.AccountsReceivable.GetAccountsReceivableStatus;

public sealed record GetAccountsReceivableStatusQuery(
    int Page = 1,
    int PageSize = 50,
    Guid? ClientId = null,
    string? Status = null // "Pending" | "PaymentVerification" | "Paid"
) : IQuery<IReadOnlyList<InvoiceReadModel>>;
