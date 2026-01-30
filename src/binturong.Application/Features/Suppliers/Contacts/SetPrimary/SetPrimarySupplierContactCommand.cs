using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.Contacts.SetPrimary;

public sealed record SetPrimarySupplierContactCommand(Guid SupplierId, Guid ContactId) : ICommand;
