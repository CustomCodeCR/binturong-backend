using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ClientAddresses;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Addresses.Remove;

internal sealed class RemoveClientAddressCommandHandler
    : ICommandHandler<RemoveClientAddressCommand>
{
    private readonly IApplicationDbContext _db;

    public RemoveClientAddressCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(RemoveClientAddressCommand command, CancellationToken ct)
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

        if (address.IsPrimary)
            return Result.Failure(ClientAddressErrors.CannotDeletePrimary);

        address.RaiseDeleted();

        _db.ClientAddresses.Remove(address);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
