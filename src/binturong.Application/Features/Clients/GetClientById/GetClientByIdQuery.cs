using Application.Abstractions.Messaging;
using Application.ReadModels.CRM;

namespace Application.Features.Clients.GetClientById;

public sealed record GetClientByIdQuery(Guid ClientId) : IQuery<ClientReadModel>;
