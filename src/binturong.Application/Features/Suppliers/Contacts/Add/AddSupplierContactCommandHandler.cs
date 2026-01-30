using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.Contacts.Add;

public sealed record AddSupplierContactCommand(
    Guid SupplierId,
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary
) : ICommand<Guid>;
