using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Contacts.SetPrimary;

public sealed record SetPrimaryClientContactCommand(Guid ClientId, Guid ContactId) : ICommand;
