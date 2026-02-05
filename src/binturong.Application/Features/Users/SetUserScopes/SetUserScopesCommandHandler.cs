using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.UserScopes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.SetUserScopes;

internal sealed class SetUserScopesCommandHandler : ICommandHandler<SetUserScopesCommand>
{
    private readonly IApplicationDbContext _db;

    public SetUserScopesCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(SetUserScopesCommand command, CancellationToken ct)
    {
        var userExists = await _db.Users.AnyAsync(x => x.Id == command.UserId, ct);
        if (!userExists)
            return Result.Failure(
                Error.NotFound("Users.NotFound", $"User '{command.UserId}' not found")
            );

        var scopes = await _db.Scopes.Where(s => command.ScopeIds.Contains(s.Id)).ToListAsync(ct);

        if (scopes.Count != command.ScopeIds.Count)
            return Result.Failure(
                Error.Failure("Users.Scopes.Invalid", "One or more scopes do not exist")
            );

        var existing = await _db.UserScopes.Where(x => x.UserId == command.UserId).ToListAsync(ct);
        var existingSet = existing.Select(x => x.ScopeId).ToHashSet();
        var desired = command.ScopeIds.ToHashSet();

        var toRemove = existing.Where(x => !desired.Contains(x.ScopeId)).ToList();
        foreach (var us in toRemove)
        {
            var sc =
                scopes.FirstOrDefault(x => x.Id == us.ScopeId)
                ?? await _db.Scopes.FirstAsync(x => x.Id == us.ScopeId, ct);

            var u = new Domain.Users.User { Id = command.UserId };
            u.Raise(new UserScopeRemovedDomainEvent(command.UserId, us.ScopeId, sc.Code));

            _db.UserScopes.Remove(us);
        }

        var toAdd = scopes.Where(s => !existingSet.Contains(s.Id)).ToList();
        foreach (var sc in toAdd)
        {
            var us = new UserScope
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                ScopeId = sc.Id,
            };

            var u = new Domain.Users.User { Id = command.UserId };
            u.Raise(new UserScopeAssignedDomainEvent(command.UserId, sc.Id, sc.Code));

            _db.UserScopes.Add(us);
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
