using Application.Abstractions.Messaging;
using Application.ReadModels.Accounting;

namespace Application.Features.Accounting.GetEntries;

public sealed record GetAccountingEntriesQuery(
    int Page,
    int PageSize,
    string? Search,
    string? EntryType
) : IQuery<IReadOnlyList<AccountingEntryReadModel>>;
