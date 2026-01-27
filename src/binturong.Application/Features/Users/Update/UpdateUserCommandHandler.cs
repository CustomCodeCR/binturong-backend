using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.Update;

internal sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateUserCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == command.UserId, ct);
        if (user is null)
            return Result.Failure(UserErrors.NotFound(command.UserId));

        var email = command.Email.Trim().ToLowerInvariant();
        var username = command.Username.Trim();

        // Email unique (except self)
        var emailExists = await _db.Users.AnyAsync(
            x => x.Id != command.UserId && x.Email.ToLower() == email,
            ct
        );
        if (emailExists)
            return Result.Failure(UserErrors.EmailNotUnique);

        // Username unique (except self)
        var usernameExists = await _db.Users.AnyAsync(
            x => x.Id != command.UserId && x.Username == username,
            ct
        );
        if (usernameExists)
            return Result.Failure(UserErrors.UsernameNotUnique);

        user.Username = username;
        user.Email = email;
        user.IsActive = command.IsActive;
        user.LastLogin = command.LastLogin;
        user.MustChangePassword = command.MustChangePassword;
        user.FailedAttempts = command.FailedAttempts;
        user.LockedUntil = command.LockedUntil;
        user.UpdatedAt = DateTime.UtcNow;

        // Domain event -> Outbox -> Mongo projection
        user.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
