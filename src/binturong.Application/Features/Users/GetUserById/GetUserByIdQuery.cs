using Application.Abstractions.Messaging;
using Application.ReadModels.Security;

namespace Application.Features.Users.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserReadModel>;
