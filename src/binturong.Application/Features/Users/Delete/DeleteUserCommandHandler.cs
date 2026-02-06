using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.Delete;

internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private const string Module = "Users";
    private const string Entity = "User";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteUserCommandHandler(
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

    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == command.UserId, ct);
        if (user is null)
            return Result.Failure(UserErrors.NotFound(command.UserId));

        var before =
            $"userId={user.Id}; username={user.Username}; email={user.Email}; isActive={user.IsActive}";

        user.RaiseDeleted();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                command.UserId,
                "USER_DELETED",
                before,
                string.Empty,
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success();
    }
}
