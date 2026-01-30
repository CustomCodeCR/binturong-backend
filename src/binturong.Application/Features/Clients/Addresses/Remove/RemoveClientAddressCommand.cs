using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Addresses.Remove;

public sealed record RemoveClientAddressCommand(Guid ClientId, Guid AddressId) : ICommand;
