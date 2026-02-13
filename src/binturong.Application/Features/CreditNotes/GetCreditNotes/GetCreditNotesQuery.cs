using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.CreditNotes.GetCreditNotes;

public sealed record GetCreditNotesQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<CreditNoteReadModel>>;
