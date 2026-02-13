using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.CreditNotes.GetCreditNoteById;

public sealed record GetCreditNoteByIdQuery(Guid Id) : IQuery<CreditNoteReadModel>;
