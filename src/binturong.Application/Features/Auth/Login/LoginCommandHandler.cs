using Application.Abstractions.Authentication;
using Application.Abstractions.Background;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Application.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IRealtimeNotifier _realtime;
    private readonly IBackgroundJobScheduler _jobs;
    private readonly EmailOptions _emailOptions;

    public LoginCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        IPermissionService permissions,
        IRequestContext request,
        ICommandBus bus,
        IRealtimeNotifier realtime,
        IBackgroundJobScheduler jobs,
        EmailOptions emailOptions
    )
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _permissions = permissions;
        _request = request;
        _bus = bus;
        _realtime = realtime;
        _jobs = jobs;
        _emailOptions = emailOptions;
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

        if (user is null)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    null,
                    "Auth",
                    "User",
                    null,
                    "LOGIN_FAILED",
                    string.Empty,
                    $"usernameOrEmail={command.UsernameOrEmail}",
                    ip,
                    userAgent
                ),
                ct
            );

            return Result.Failure<LoginResponse>(LoginErrors.InvalidCredentials);
        }

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

            if (user.FailedAttempts == 5)
            {
                var now = DateTime.UtcNow;

                await _realtime.NotifyRoleAsync(
                    "Admin",
                    "security.suspicious_login",
                    new
                    {
                        userId = user.Id,
                        username = user.Username,
                        email = user.Email,
                        failedAttempts = user.FailedAttempts,
                        lockedUntil = user.LockedUntil,
                        ip,
                        userAgent,
                        atUtc = now,
                    },
                    ct
                );

                var to = _emailOptions.AdminEmail ?? "admin@local";
                await _jobs.EnqueueAsync(
                    async (sp, token) =>
                    {
                        var email = sp.GetRequiredService<IEmailSender>();
                        await email.SendAsync(
                            to,
                            "Alerta: actividad sospechosa (login)",
                            $@"
<h3>Actividad sospechosa</h3>
<p><b>User:</b> {user.Username} ({user.Email})</p>
<p><b>FailedAttempts:</b> {user.FailedAttempts}</p>
<p><b>LockedUntil:</b> {user.LockedUntil:O}</p>
<p><b>IP:</b> {ip}</p>
<p><b>UserAgent:</b> {userAgent}</p>
<p><b>At:</b> {now:O}</p>
",
                            token
                        );
                    },
                    ct
                );
            }

            return Result.Failure<LoginResponse>(LoginErrors.InvalidCredentials);
        }

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

        var roles = await _permissions.GetUserRoleNamesAsync(user.Id, ct);
        var scopes = await _permissions.GetUserScopesAsync(user.Id, ct);

        var token = _jwt.Generate(user.Id, user.Username, user.Email, roles, scopes);

        return Result.Success(
            new LoginResponse(user.Id, user.Username, user.Email, token, roles, scopes)
        );
    }
}
