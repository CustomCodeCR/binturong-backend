using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.RemoveLine;

public sealed record RemoveLineDiscountCommand(Guid SalesOrderId, Guid SalesOrderDetailId)
    : ICommand;
