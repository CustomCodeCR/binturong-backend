using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.ApplyLine;

public sealed record ApplyLineDiscountCommand(
    Guid SalesOrderId,
    Guid SalesOrderDetailId,
    decimal DiscountPerc,
    string Reason,
    Guid PolicyId
) : ICommand;
