using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.Delete;

internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteUserCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == command.UserId, ct);
        if (user is null)
            return Result.Failure(UserErrors.NotFound(command.UserId));

        // Domain event -> Outbox -> Mongo projection
        user.RaiseDeleted();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
