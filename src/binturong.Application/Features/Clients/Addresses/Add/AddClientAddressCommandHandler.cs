using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ClientAddresses;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Addresses.Add;

internal sealed class AddClientAddressCommandHandler
    : ICommandHandler<AddClientAddressCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public AddClientAddressCommandHandler(
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

    public async Task<Result<Guid>> Handle(AddClientAddressCommand command, CancellationToken ct)
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
                null,
                "CLIENT_ADDRESS_ADD_FAILED",
                string.Empty,
                $"reason=client_not_found; clientId={command.ClientId}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(ClientErrors.NotFound(command.ClientId));
        }

        var now = DateTime.UtcNow;

        // enforce only one primary
        if (command.IsPrimary)
        {
            var primaries = await _db
                .ClientAddresses.Where(x => x.ClientId == command.ClientId && x.IsPrimary)
                .ToListAsync(ct);

            foreach (var p in primaries)
            {
                p.IsPrimary = false;
                p.UpdatedAt = now;
                p.RaiseUpdated();
            }
        }

        var address = new ClientAddress
        {
            Id = Guid.NewGuid(),
            ClientId = command.ClientId,
            AddressType = command.AddressType.Trim(),
            AddressLine = command.AddressLine.Trim(),
            Province = command.Province.Trim(),
            Canton = command.Canton.Trim(),
            District = command.District.Trim(),
            Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim(),
            IsPrimary = command.IsPrimary,
            CreatedAt = now,
            UpdatedAt = now,
        };

        address.RaiseCreated();

        _db.ClientAddresses.Add(address);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Clients",
            "ClientAddress",
            address.Id,
            "CLIENT_ADDRESS_ADDED",
            string.Empty,
            $"clientId={command.ClientId}; addressId={address.Id}; type={address.AddressType}; isPrimary={address.IsPrimary}; province={address.Province}; canton={address.Canton}; district={address.District}",
            ip,
            ua,
            ct
        );

        return Result.Success(address.Id);
    }
}
