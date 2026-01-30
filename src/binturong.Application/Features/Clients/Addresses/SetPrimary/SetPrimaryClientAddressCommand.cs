using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Addresses.SetPrimary;

public sealed record SetPrimaryClientAddressCommand(Guid ClientId, Guid AddressId) : ICommand;
