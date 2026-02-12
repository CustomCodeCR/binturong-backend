using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.History.ExportSupplierPurchaseHistoryExcel;

public sealed record ExportSupplierPurchaseHistoryExcelCommand(
    Guid SupplierId,
    DateTime? From,
    DateTime? To,
    string? Status
) : ICommand<byte[]>;
