using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Auth.Login;

internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IPermissionService _permissions;
    private readonly IRequestContext _request;
    private readonly ICommandBus _bus;

    public LoginCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        IPermissionService permissions,
        IRequestContext request,
        ICommandBus bus
    )
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _permissions = permissions;
        _request = request;
        _bus = bus;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var userAgent = _request.UserAgent;

        var key = command.UsernameOrEmail.Trim();
        var normalizedEmail = key.ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(
            x => x.Username == key || x.Email.ToLower() == normalizedEmail,
            ct
        );

        // =========================
        // User not found
        // =========================
        if (user is null)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    null, // UserId
                    "Auth", // Module
                    "User", // Entity
                    null, // EntityId
                    "LOGIN_FAILED", // Action
                    string.Empty, // DataBefore
                    $"usernameOrEmail={command.UsernameOrEmail}", // DataAfter
                    ip, // Ip
                    userAgent // UserAgent
                ),
                ct
            );

            return Result.Failure<LoginResponse>(LoginErrors.InvalidCredentials);
        }

        // =========================
        // User inactive
        // =========================
        if (!user.IsActive)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    user.Id,
                    "Auth",
                    "User",
                    user.Id,
                    "LOGIN_BLOCKED_INACTIVE",
                    string.Empty,
                    $"userId={user.Id}; username={user.Username}; email={user.Email}",
                    ip,
                    userAgent
                ),
                ct
            );

            return Result.Failure<LoginResponse>(LoginErrors.UserInactive);
        }

        // =========================
        // User locked
        // =========================
        if (user.LockedUntil is not null && user.LockedUntil.Value > DateTime.UtcNow)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    user.Id,
                    "Auth",
                    "User",
                    user.Id,
                    "LOGIN_BLOCKED_LOCKED",
                    string.Empty,
                    $"userId={user.Id}; lockedUntil={user.LockedUntil}",
                    ip,
                    userAgent
                ),
                ct
            );

            return Result.Failure<LoginResponse>(LoginErrors.UserLocked);
        }

        // =========================
        // Password validation
        // =========================
        if (!_hasher.Verify(command.Password, user.PasswordHash))
        {
            user.FailedAttempts += 1;

            if (user.FailedAttempts >= 5)
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);

            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            await _bus.Send(
                new CreateAuditLogCommand(
                    user.Id,
                    "Auth",
                    "User",
                    user.Id,
                    "LOGIN_FAILED",
                    string.Empty,
                    $"userId={user.Id}; failedAttempts={user.FailedAttempts}; lockedUntil={user.LockedUntil}",
                    ip,
                    userAgent
                ),
                ct
            );

            return Result.Failure<LoginResponse>(LoginErrors.InvalidCredentials);
        }

        // =========================
        // Successful login
        // =========================
        user.FailedAttempts = 0;
        user.LockedUntil = null;
        user.LastLogin = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                user.Id,
                "Auth",
                "User",
                user.Id,
                "LOGIN_SUCCESS",
                string.Empty,
                $"userId={user.Id}; username={user.Username}; email={user.Email}",
                ip,
                userAgent
            ),
            ct
        );

        // =========================
        // Token generation
        // =========================
        var roles = await _permissions.GetUserRoleNamesAsync(user.Id, ct);
        var scopes = await _permissions.GetUserScopesAsync(user.Id, ct);

        var token = _jwt.Generate(user.Id, user.Username, user.Email, roles, scopes);

        return Result.Success(
            new LoginResponse(user.Id, user.Username, user.Email, token, roles, scopes)
        );
    }
}
