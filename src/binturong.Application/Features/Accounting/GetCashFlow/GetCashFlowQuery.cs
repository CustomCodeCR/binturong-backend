using Application.Abstractions.Messaging;
using Application.ReadModels.Accounting;

namespace Application.Features.Accounting.GetCashFlow;

public sealed record GetCashFlowQuery(DateTime FromUtc, DateTime ToUtc) : IQuery<CashFlowReadModel>;
