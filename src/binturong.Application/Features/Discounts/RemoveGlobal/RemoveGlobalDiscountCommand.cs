using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.RemoveGlobal;

public sealed record RemoveGlobalDiscountCommand(Guid SalesOrderId) : ICommand;
