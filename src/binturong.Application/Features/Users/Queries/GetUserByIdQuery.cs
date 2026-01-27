using Application.Abstractions.Messaging;
using Application.ReadModels.Security;

namespace Application.Features.Users.Queries;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserReadModel>;
