using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Roles.Create;

internal sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateRoleCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateRoleCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        var name = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            await _bus.AuditAsync(
                userId,
                "Roles",
                "Role",
                null,
                "ROLE_CREATE_FAILED",
                string.Empty,
                "reason=invalid_name",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(RoleErrors.InvalidName);
        }

        var exists = await _db.Roles.AnyAsync(x => x.Name.ToLower() == name.ToLower(), ct);
        if (exists)
        {
            await _bus.AuditAsync(
                userId,
                "Roles",
                "Role",
                null,
                "ROLE_CREATE_FAILED",
                string.Empty,
                $"reason=name_not_unique; name={name}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(RoleErrors.NameNotUnique);
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = command.Description?.Trim(),
            IsActive = command.IsActive,
        };

        role.RaiseCreated();

        _db.Roles.Add(role);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Roles",
            "Role",
            role.Id,
            "ROLE_CREATED",
            string.Empty,
            $"roleId={role.Id}; name={role.Name}; isActive={role.IsActive}",
            ip,
            ua,
            ct
        );

        return Result.Success(role.Id);
    }
}
