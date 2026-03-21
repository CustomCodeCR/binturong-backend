using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.ApplyGlobal;

public sealed record ApplyGlobalDiscountCommand(
    Guid SalesOrderId,
    decimal DiscountPerc,
    string Reason,
    Guid PolicyId
) : ICommand;
