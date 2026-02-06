using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Delete;

internal sealed class DeleteClientCommandHandler : ICommandHandler<DeleteClientCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteClientCommandHandler(
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

    public async Task<Result> Handle(DeleteClientCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var client = await _db.Clients.FirstOrDefaultAsync(x => x.Id == command.ClientId, ct);
        if (client is null)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "Client",
                command.ClientId,
                "CLIENT_DELETE_FAILED",
                string.Empty,
                $"reason=not_found; clientId={command.ClientId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientErrors.NotFound(command.ClientId));
        }

        var before =
            $"tradeName={client.TradeName}; email={client.Email}; identification={client.Identification}; isActive={client.IsActive}";

        client.RaiseDeleted();

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Clients",
            "Client",
            client.Id,
            "CLIENT_DELETED",
            before,
            $"clientId={client.Id}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
