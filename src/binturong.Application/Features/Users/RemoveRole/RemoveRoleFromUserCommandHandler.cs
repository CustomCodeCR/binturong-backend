using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.RemoveRole;

internal sealed class RemoveRoleFromUserCommandHandler : ICommandHandler<RemoveRoleFromUserCommand>
{
    private readonly IApplicationDbContext _db;

    public RemoveRoleFromUserCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(RemoveRoleFromUserCommand command, CancellationToken ct)
    {
        var ur = await _db.UserRoles.FirstOrDefaultAsync(
            x => x.UserId == command.UserId && x.RoleId == command.RoleId,
            ct
        );

        if (ur is null)
            return Result.Success();

        _db.UserRoles.Remove(ur);

        var u = new Domain.Users.User { Id = command.UserId };
        u.Raise(new Domain.UserRoles.UserRoleRemovedDomainEvent(command.UserId, command.RoleId));

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
