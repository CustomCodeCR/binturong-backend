using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Contacts.Add;

public sealed record AddClientContactCommand(
    Guid ClientId,
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary
) : ICommand<Guid>;
