using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ClientAddresses;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Addresses.Add;

internal sealed class AddClientAddressCommandHandler
    : ICommandHandler<AddClientAddressCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public AddClientAddressCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(AddClientAddressCommand command, CancellationToken ct)
    {
        var clientExists = await _db.Clients.AnyAsync(x => x.Id == command.ClientId, ct);
        if (!clientExists)
            return Result.Failure<Guid>(ClientErrors.NotFound(command.ClientId));

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

        return Result.Success(address.Id);
    }
}
