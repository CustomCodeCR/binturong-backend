using Application.Abstractions.Messaging;
using Application.ReadModels.Accounting;

namespace Application.Features.Accounting.GetIncomeStatement;

public sealed record GetIncomeStatementQuery(DateTime FromUtc, DateTime ToUtc)
    : IQuery<IncomeStatementReadModel>;
