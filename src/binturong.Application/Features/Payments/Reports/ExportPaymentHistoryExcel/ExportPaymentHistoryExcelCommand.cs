using Application.Abstractions.Messaging;

namespace Application.Features.Payments.Reports.ExportPaymentHistoryExcel;

public sealed record ExportPaymentHistoryExcelCommand(
    DateTime? From,
    DateTime? To,
    Guid? ClientId,
    Guid? PaymentMethodId,
    string? Search
) : ICommand<byte[]>;
