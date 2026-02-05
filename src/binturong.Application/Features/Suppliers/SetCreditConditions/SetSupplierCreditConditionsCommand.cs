using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.SetCreditConditions;

public sealed record SetSupplierCreditConditionsCommand(
    Guid SupplierId,
    decimal CreditLimit,
    int CreditDays,
    bool HasPermission
) : ICommand;
