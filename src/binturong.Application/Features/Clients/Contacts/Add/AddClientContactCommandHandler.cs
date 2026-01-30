using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ClientContacts;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Contacts.Add;

internal sealed class AddClientContactCommandHandler
    : ICommandHandler<AddClientContactCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public AddClientContactCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(AddClientContactCommand command, CancellationToken ct)
    {
        var clientExists = await _db.Clients.AnyAsync(x => x.Id == command.ClientId, ct);
        if (!clientExists)
            return Result.Failure<Guid>(ClientErrors.NotFound(command.ClientId));

        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Guid>(ClientContactErrors.EmailIsRequired);

        // email unique per client
        var emailExists = await _db.ClientContacts.AnyAsync(
            x => x.ClientId == command.ClientId && x.Email.ToLower() == email,
            ct
        );
        if (emailExists)
            return Result.Failure<Guid>(ClientContactErrors.EmailNotUniqueForClient);

        var now = DateTime.UtcNow;

        if (command.IsPrimary)
        {
            var primaries = await _db
                .ClientContacts.Where(x => x.ClientId == command.ClientId && x.IsPrimary)
                .ToListAsync(ct);

            foreach (var p in primaries)
            {
                p.IsPrimary = false;
                p.UpdatedAt = now;
                p.RaiseUpdated();
            }
        }

        var contact = new ClientContact
        {
            Id = Guid.NewGuid(),
            ClientId = command.ClientId,
            Name = command.Name.Trim(),
            JobTitle = string.IsNullOrWhiteSpace(command.JobTitle) ? null : command.JobTitle.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim(),
            IsPrimary = command.IsPrimary,
            CreatedAt = now,
            UpdatedAt = now,
        };

        contact.RaiseCreated();

        _db.ClientContacts.Add(contact);
        await _db.SaveChangesAsync(ct);

        return Result.Success(contact.Id);
    }
}
