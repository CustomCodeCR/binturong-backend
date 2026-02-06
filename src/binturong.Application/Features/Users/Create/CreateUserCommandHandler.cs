using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.Create;

internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private const string Module = "Users";
    private const string Entity = "User";

    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateUserCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var username = command.Username.Trim();

        var emailExists = await _db.Users.AnyAsync(x => x.Email.ToLower() == email, ct);
        if (emailExists)
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);

        var usernameExists = await _db.Users.AnyAsync(x => x.Username == username, ct);
        if (usernameExists)
            return Result.Failure<Guid>(UserErrors.UsernameNotUnique);

        var now = DateTime.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = _passwordHasher.Hash(command.Password),
            IsActive = command.IsActive,
            LastLogin = null,
            MustChangePassword = true,
            FailedAttempts = 0,
            LockedUntil = null,
            CreatedAt = now,
            UpdatedAt = now,
        };

        user.RaiseRegistered();

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                user.Id,
                "USER_CREATED",
                string.Empty,
                $"userId={user.Id}; username={user.Username}; email={user.Email}; isActive={user.IsActive}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success(user.Id);
    }
}
