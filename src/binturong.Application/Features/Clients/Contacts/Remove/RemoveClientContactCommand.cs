using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Contacts.Remove;

public sealed record RemoveClientContactCommand(Guid ClientId, Guid ContactId) : ICommand;
