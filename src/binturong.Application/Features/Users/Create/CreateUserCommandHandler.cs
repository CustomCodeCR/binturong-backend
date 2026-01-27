using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.Create;

internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var username = command.Username.Trim();

        // Uniqueness checks (simple)
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

        // Domain event -> Outbox -> Mongo projection
        user.RaiseRegistered();

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return Result.Success(user.Id);
    }
}
