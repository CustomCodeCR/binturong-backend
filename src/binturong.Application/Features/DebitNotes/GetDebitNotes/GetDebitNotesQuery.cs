using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.DebitNotes.GetDebitNotes;

public sealed record GetDebitNotesQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<DebitNoteReadModel>>;
