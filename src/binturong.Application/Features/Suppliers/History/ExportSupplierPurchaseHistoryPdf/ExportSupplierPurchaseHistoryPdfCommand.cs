using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.History.ExportSupplierPurchaseHistoryPdf;

public sealed record ExportSupplierPurchaseHistoryPdfCommand(
    Guid SupplierId,
    DateTime? From,
    DateTime? To,
    string? Status
) : ICommand<byte[]>;
