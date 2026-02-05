using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Roles.Create;

internal sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateRoleCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateRoleCommand command, CancellationToken ct)
    {
        var name = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Guid>(RoleErrors.InvalidName);

        var exists = await _db.Roles.AnyAsync(x => x.Name.ToLower() == name.ToLower(), ct);
        if (exists)
            return Result.Failure<Guid>(RoleErrors.NameNotUnique);

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

        return Result.Success(role.Id);
    }
}
