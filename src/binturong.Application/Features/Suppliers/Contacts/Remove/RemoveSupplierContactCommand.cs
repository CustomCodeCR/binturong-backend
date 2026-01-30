using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.Contacts.Remove;

public sealed record RemoveSupplierContactCommand(Guid SupplierId, Guid ContactId) : ICommand;
