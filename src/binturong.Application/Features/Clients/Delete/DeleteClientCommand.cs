using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Delete;

public sealed record DeleteClientCommand(Guid ClientId) : ICommand;
