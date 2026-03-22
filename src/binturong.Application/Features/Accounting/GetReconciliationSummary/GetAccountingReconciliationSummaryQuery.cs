using Application.Abstractions.Messaging;
using Application.ReadModels.Accounting;

namespace Application.Features.Accounting.GetReconciliationSummary;

public sealed record GetAccountingReconciliationSummaryQuery()
    : IQuery<AccountingReconciliationSummaryReadModel>;
