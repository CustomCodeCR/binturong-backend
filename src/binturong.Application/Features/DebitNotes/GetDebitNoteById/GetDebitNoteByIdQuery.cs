using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.DebitNotes.GetDebitNoteById;

public sealed record GetDebitNoteByIdQuery(Guid Id) : IQuery<DebitNoteReadModel>;
