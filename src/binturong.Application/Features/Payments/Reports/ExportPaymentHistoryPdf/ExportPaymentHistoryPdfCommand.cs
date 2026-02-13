using Application.Abstractions.Messaging;

namespace Application.Features.Payments.Reports.ExportPaymentHistoryPdf;

public sealed record ExportPaymentHistoryPdfCommand(
    DateTime? From,
    DateTime? To,
    Guid? ClientId,
    Guid? PaymentMethodId,
    string? Search
) : ICommand<byte[]>;
