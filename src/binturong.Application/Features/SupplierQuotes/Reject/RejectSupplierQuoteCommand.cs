using Application.Abstractions.Messaging;

namespace Application.Features.SupplierQuotes.Reject;

public sealed record RejectSupplierQuoteCommand(
    Guid SupplierQuoteId,
    string Reason,
    DateTime RejectedAtUtc
) : ICommand;
