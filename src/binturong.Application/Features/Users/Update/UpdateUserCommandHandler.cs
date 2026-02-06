using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.Update;

internal sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private const string Module = "Users";
    private const string Entity = "User";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateUserCommandHandler(
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

    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == command.UserId, ct);
        if (user is null)
            return Result.Failure(UserErrors.NotFound(command.UserId));

        var before =
            $"userId={user.Id}; username={user.Username}; email={user.Email}; isActive={user.IsActive}; mustChangePassword={user.MustChangePassword}; failedAttempts={user.FailedAttempts}; lockedUntil={user.LockedUntil}";

        var email = command.Email.Trim().ToLowerInvariant();
        var username = command.Username.Trim();

        var emailExists = await _db.Users.AnyAsync(
            x => x.Id != command.UserId && x.Email.ToLower() == email,
            ct
        );
        if (emailExists)
            return Result.Failure(UserErrors.EmailNotUnique);

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

        user.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        var after =
            $"userId={user.Id}; username={user.Username}; email={user.Email}; isActive={user.IsActive}; mustChangePassword={user.MustChangePassword}; failedAttempts={user.FailedAttempts}; lockedUntil={user.LockedUntil}";

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                command.UserId,
                "USER_UPDATED",
                before,
                after,
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success();
    }
}
