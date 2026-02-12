using Application.Abstractions.Messaging;

namespace Application.Features.Clients.History.ExportClientHistoryPdf;

public sealed record ExportClientHistoryPdfCommand(
    Guid ClientId,
    DateTime? From,
    DateTime? To,
    string? Status
) : ICommand<byte[]>;
