using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Invoices.GetInvoicesSelect;

public sealed record GetInvoicesSelectQuery(string? Search)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
