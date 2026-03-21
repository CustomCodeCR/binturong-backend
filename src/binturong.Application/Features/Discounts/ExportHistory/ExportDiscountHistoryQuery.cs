using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.ExportHistory;

public sealed record ExportDiscountHistoryQuery(
    string? Search,
    Guid? UserId,
    DateTime? FromUtc,
    DateTime? ToUtc
) : IQuery<ExportDiscountHistoryResponse>;

public sealed record ExportDiscountHistoryResponse(
    string FileName,
    string ContentType,
    byte[] Content
);
