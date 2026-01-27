using Application.Abstractions.Messaging;
using Application.ReadModels.Security;

namespace Application.Features.Users.Queries;

public sealed record ListUsersQuery(int Take) : IQuery<IReadOnlyList<UserReadModel>>;
