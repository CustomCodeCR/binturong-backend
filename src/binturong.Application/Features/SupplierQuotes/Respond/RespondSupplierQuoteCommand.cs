using Application.Abstractions.Messaging;
using Domain.SupplierQuotes;

namespace Application.Features.SupplierQuotes.Respond;

public sealed record RespondSupplierQuoteCommand(
    Guid SupplierQuoteId,
    DateTime RespondedAtUtc,
    string? SupplierMessage,
    IReadOnlyList<SupplierQuoteResponseLineDto> Lines
) : ICommand;
