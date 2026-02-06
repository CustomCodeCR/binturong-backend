using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ClientAddresses;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Addresses.SetPrimary;

internal sealed class SetPrimaryClientAddressCommandHandler
    : ICommandHandler<SetPrimaryClientAddressCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public SetPrimaryClientAddressCommandHandler(
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

    public async Task<Result> Handle(SetPrimaryClientAddressCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var clientExists = await _db.Clients.AnyAsync(x => x.Id == command.ClientId, ct);
        if (!clientExists)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientAddress",
                command.AddressId,
                "CLIENT_ADDRESS_PRIMARY_SET_FAILED",
                string.Empty,
                $"reason=client_not_found; clientId={command.ClientId}; addressId={command.AddressId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientErrors.NotFound(command.ClientId));
        }

        var address = await _db.ClientAddresses.FirstOrDefaultAsync(
            x => x.Id == command.AddressId && x.ClientId == command.ClientId,
            ct
        );

        if (address is null)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientAddress",
                command.AddressId,
                "CLIENT_ADDRESS_PRIMARY_SET_FAILED",
                string.Empty,
                $"reason=not_found; clientId={command.ClientId}; addressId={command.AddressId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientAddressErrors.NotFound(command.AddressId));
        }

        var now = DateTime.UtcNow;

        var others = await _db
            .ClientAddresses.Where(x =>
                x.ClientId == command.ClientId && x.IsPrimary && x.Id != command.AddressId
            )
            .ToListAsync(ct);

        foreach (var o in others)
        {
            o.IsPrimary = false;
            o.UpdatedAt = now;
            o.RaiseUpdated();
        }

        address.IsPrimary = true;
        address.UpdatedAt = now;

        address.RaiseUpdated();
        address.RaisePrimarySet();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Clients",
            "ClientAddress",
            address.Id,
            "CLIENT_ADDRESS_PRIMARY_SET",
            string.Empty,
            $"clientId={command.ClientId}; addressId={address.Id}; isPrimary=true",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
