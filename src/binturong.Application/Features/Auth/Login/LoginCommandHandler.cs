using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Auth.Login;

internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IPermissionService _permissions;

    public LoginCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        IPermissionService permissions
    )
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _permissions = permissions;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken ct)
    {
        var key = command.UsernameOrEmail.Trim();
        var normalizedEmail = key.ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(
            x => x.Username == key || x.Email.ToLower() == normalizedEmail,
            ct
        );

        if (user is null)
            return Result.Failure<LoginResponse>(LoginErrors.InvalidCredentials);

        if (!user.IsActive)
            return Result.Failure<LoginResponse>(LoginErrors.UserInactive);

        if (user.LockedUntil is not null && user.LockedUntil.Value > DateTime.UtcNow)
            return Result.Failure<LoginResponse>(LoginErrors.UserLocked);

        // ✅ FIX: Verify(password, hash)  (NOT hash, password)
        if (!_hasher.Verify(command.Password, user.PasswordHash))
        {
            user.FailedAttempts += 1;

            if (user.FailedAttempts >= 5)
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);

            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return Result.Failure<LoginResponse>(LoginErrors.InvalidCredentials);
        }

        user.FailedAttempts = 0;
        user.LockedUntil = null;
        user.LastLogin = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var roles = await _permissions.GetUserRoleNamesAsync(user.Id, ct);
        var scopes = await _permissions.GetUserScopesAsync(user.Id, ct);

        var token = _jwt.Generate(user.Id, user.Username, user.Email, roles, scopes);

        return Result.Success(
            new LoginResponse(user.Id, user.Username, user.Email, token, roles, scopes)
        );
    }
}
