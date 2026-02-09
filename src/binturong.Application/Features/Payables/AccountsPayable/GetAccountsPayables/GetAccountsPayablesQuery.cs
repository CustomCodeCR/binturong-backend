using Application.Abstractions.Messaging;
using Application.ReadModels.Payables;

namespace Application.Features.Payables.AccountsPayable.GetAccountsPayables;

public sealed record GetAccountsPayablesQuery(
    int Page,
    int PageSize,
    bool? OnlyOverdue,
    string? Search
) : IQuery<IReadOnlyList<AccountsPayableReadModel>>
{
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}
