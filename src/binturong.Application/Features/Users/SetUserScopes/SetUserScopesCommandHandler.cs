using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.UserScopes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.SetUserScopes;

internal sealed class SetUserScopesCommandHandler : ICommandHandler<SetUserScopesCommand>
{
    private const string Module = "Users";
    private const string Entity = "User";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public SetUserScopesCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

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

        var removed = new List<Guid>();
        var added = new List<Guid>();

        var toRemove = existing.Where(x => !desired.Contains(x.ScopeId)).ToList();
        foreach (var us in toRemove)
        {
            var sc =
                scopes.FirstOrDefault(x => x.Id == us.ScopeId)
                ?? await _db.Scopes.FirstAsync(x => x.Id == us.ScopeId, ct);

            var u = new Domain.Users.User { Id = command.UserId };
            u.Raise(new UserScopeRemovedDomainEvent(command.UserId, us.ScopeId, sc.Code));

            _db.UserScopes.Remove(us);
            removed.Add(us.ScopeId);
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
            added.Add(sc.Id);
        }

        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                command.UserId,
                "USER_SCOPES_SET",
                string.Empty,
                $"targetUserId={command.UserId}; added=[{string.Join(",", added)}]; removed=[{string.Join(",", removed)}]; desiredCount={command.ScopeIds.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success();
    }
}
