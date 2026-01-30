using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Addresses.Update;

public sealed record UpdateClientAddressCommand(
    Guid ClientId,
    Guid AddressId,
    string AddressType,
    string AddressLine,
    string Province,
    string Canton,
    string District,
    string? Notes,
    bool IsPrimary
) : ICommand;
