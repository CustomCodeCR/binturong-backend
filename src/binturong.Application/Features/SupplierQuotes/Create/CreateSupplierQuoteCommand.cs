using Application.Abstractions.Messaging;

namespace Application.Features.SupplierQuotes.Create;

public sealed record CreateSupplierQuoteCommand(
    string Code,
    Guid SupplierId,
    Guid? BranchId,
    DateTime RequestedAtUtc,
    string? Notes,
    IReadOnlyList<CreateSupplierQuoteLineDto> Lines
) : ICommand<Guid>;

public sealed record CreateSupplierQuoteLineDto(Guid ProductId, decimal Quantity);
