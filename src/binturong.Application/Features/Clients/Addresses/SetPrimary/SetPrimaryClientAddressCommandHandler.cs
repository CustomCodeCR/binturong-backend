using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ClientAddresses;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Addresses.SetPrimary;

internal sealed class SetPrimaryClientAddressCommandHandler
    : ICommandHandler<SetPrimaryClientAddressCommand>
{
    private readonly IApplicationDbContext _db;

    public SetPrimaryClientAddressCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(SetPrimaryClientAddressCommand command, CancellationToken ct)
    {
        var clientExists = await _db.Clients.AnyAsync(x => x.Id == command.ClientId, ct);
        if (!clientExists)
            return Result.Failure(ClientErrors.NotFound(command.ClientId));

        var address = await _db.ClientAddresses.FirstOrDefaultAsync(
            x => x.Id == command.AddressId && x.ClientId == command.ClientId,
            ct
        );

        if (address is null)
            return Result.Failure(ClientAddressErrors.NotFound(command.AddressId));

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

        return Result.Success();
    }
}
