using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Addresses.Add;

public sealed record AddClientAddressCommand(
    Guid ClientId,
    string AddressType,
    string AddressLine,
    string Province,
    string Canton,
    string District,
    string? Notes,
    bool IsPrimary
) : ICommand<Guid>;
