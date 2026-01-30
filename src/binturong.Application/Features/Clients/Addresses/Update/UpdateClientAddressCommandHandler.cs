using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ClientAddresses;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Addresses.Update;

internal sealed class UpdateClientAddressCommandHandler
    : ICommandHandler<UpdateClientAddressCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateClientAddressCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateClientAddressCommand command, CancellationToken ct)
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

        return Result.Success();
    }
}
