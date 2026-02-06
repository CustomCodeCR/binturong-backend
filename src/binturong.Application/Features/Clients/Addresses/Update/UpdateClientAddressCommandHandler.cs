using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ClientAddresses;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Addresses.Update;

internal sealed class UpdateClientAddressCommandHandler
    : ICommandHandler<UpdateClientAddressCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateClientAddressCommandHandler(
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

    public async Task<Result> Handle(UpdateClientAddressCommand command, CancellationToken ct)
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
                "CLIENT_ADDRESS_UPDATE_FAILED",
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
                "CLIENT_ADDRESS_UPDATE_FAILED",
                string.Empty,
                $"reason=not_found; clientId={command.ClientId}; addressId={command.AddressId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientAddressErrors.NotFound(command.AddressId));
        }

        var before =
            $"type={address.AddressType}; line={address.AddressLine}; province={address.Province}; canton={address.Canton}; district={address.District}; notes={address.Notes}; isPrimary={address.IsPrimary}";

        var now = DateTime.UtcNow;

        // enforce only one primary
        if (command.IsPrimary)
        {
            var primaries = await _db
                .ClientAddresses.Where(x =>
                    x.ClientId == command.ClientId && x.IsPrimary && x.Id != command.AddressId
                )
                .ToListAsync(ct);

            foreach (var p in primaries)
            {
                p.IsPrimary = false;
                p.UpdatedAt = now;
                p.RaiseUpdated();
            }
        }

        address.AddressType = command.AddressType.Trim();
        address.AddressLine = command.AddressLine.Trim();
        address.Province = command.Province.Trim();
        address.Canton = command.Canton.Trim();
        address.District = command.District.Trim();
        address.Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim();
        address.IsPrimary = command.IsPrimary;
        address.UpdatedAt = now;

        address.RaiseUpdated();

        if (command.IsPrimary)
            address.RaisePrimarySet();

        await _db.SaveChangesAsync(ct);

        var after =
            $"type={address.AddressType}; line={address.AddressLine}; province={address.Province}; canton={address.Canton}; district={address.District}; notes={address.Notes}; isPrimary={address.IsPrimary}";

        await _bus.AuditAsync(
            userId,
            "Clients",
            "ClientAddress",
            address.Id,
            "CLIENT_ADDRESS_UPDATED",
            before,
            after,
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
