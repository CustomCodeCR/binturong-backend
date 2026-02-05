using Application.Abstractions.Messaging;
using Application.ReadModels.Security;

namespace Application.Features.Roles.GetRoleById;

public sealed record GetRoleByIdQuery(Guid RoleId) : IQuery<RoleReadModel>;
