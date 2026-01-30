using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Contacts.Update;

public sealed record UpdateClientContactCommand(
    Guid ClientId,
    Guid ContactId,
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary
) : ICommand;
