using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.Contacts.Update;

public sealed record UpdateSupplierContactCommand(
    Guid SupplierId,
    Guid ContactId,
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary
) : ICommand;
