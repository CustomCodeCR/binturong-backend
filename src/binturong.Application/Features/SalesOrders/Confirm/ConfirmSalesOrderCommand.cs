using Application.Abstractions.Messaging;

namespace Application.Features.SalesOrders.Confirm;

public sealed record ConfirmSalesOrderCommand(Guid SalesOrderId, Guid SellerUserId) : ICommand;
