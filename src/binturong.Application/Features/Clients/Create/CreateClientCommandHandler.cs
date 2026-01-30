using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Create;

internal sealed class CreateClientCommandHandler : ICommandHandler<CreateClientCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateClientCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateClientCommand command, CancellationToken ct)
    {
        var identification = command.Identification.Trim();
        var email = command.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(identification))
            return Result.Failure<Guid>(ClientErrors.IdentificationIsRequired);

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Guid>(ClientErrors.EmailIsRequired);

        if (string.IsNullOrWhiteSpace(command.TradeName))
            return Result.Failure<Guid>(ClientErrors.TradeNameIsRequired);

        if (string.IsNullOrWhiteSpace(command.PrimaryPhone))
            return Result.Failure<Guid>(ClientErrors.PrimaryPhoneIsRequired);

        var emailExists = await _db.Clients.AnyAsync(x => x.Email.ToLower() == email, ct);
        if (emailExists)
            return Result.Failure<Guid>(ClientErrors.EmailNotUnique);

        var identificationExists = await _db.Clients.AnyAsync(
            x => x.Identification == identification,
            ct
        );
        if (identificationExists)
            return Result.Failure<Guid>(ClientErrors.IdentificationNotUnique);

        var now = DateTime.UtcNow;

        var client = new Client
        {
            Id = Guid.NewGuid(),
            PersonType = command.PersonType.Trim(),
            IdentificationType = command.IdentificationType.Trim(),
            Identification = identification,
            TradeName = command.TradeName.Trim(),
            ContactName = command.ContactName.Trim(),
            Email = email,
            PrimaryPhone = command.PrimaryPhone.Trim(),
            SecondaryPhone = string.IsNullOrWhiteSpace(command.SecondaryPhone)
                ? null
                : command.SecondaryPhone.Trim(),
            Industry = string.IsNullOrWhiteSpace(command.Industry) ? null : command.Industry.Trim(),
            ClientType = string.IsNullOrWhiteSpace(command.ClientType)
                ? null
                : command.ClientType.Trim(),
            Score = command.Score,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        client.RaiseCreated();

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);

        return Result.Success(client.Id);
    }
}
