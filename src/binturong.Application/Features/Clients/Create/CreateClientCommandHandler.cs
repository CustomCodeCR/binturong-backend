using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Create;

internal sealed class CreateClientCommandHandler : ICommandHandler<CreateClientCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateClientCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateClientCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var identification = command.Identification.Trim();
        var email = command.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(identification))
            return await Fail("identification_required", ClientErrors.IdentificationIsRequired);

        if (string.IsNullOrWhiteSpace(email))
            return await Fail("email_required", ClientErrors.EmailIsRequired);

        if (string.IsNullOrWhiteSpace(command.TradeName))
            return await Fail("trade_name_required", ClientErrors.TradeNameIsRequired);

        if (string.IsNullOrWhiteSpace(command.PrimaryPhone))
            return await Fail("primary_phone_required", ClientErrors.PrimaryPhoneIsRequired);

        var emailExists = await _db.Clients.AnyAsync(x => x.Email.ToLower() == email, ct);
        if (emailExists)
            return await Fail("email_not_unique", ClientErrors.EmailNotUnique);

        var identificationExists = await _db.Clients.AnyAsync(
            x => x.Identification == identification,
            ct
        );
        if (identificationExists)
            return await Fail("identification_not_unique", ClientErrors.IdentificationNotUnique);

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

        await _bus.AuditAsync(
            userId,
            "Clients",
            "Client",
            client.Id,
            "CLIENT_CREATED",
            string.Empty,
            $"clientId={client.Id}; tradeName={client.TradeName}; email={client.Email}; identification={client.Identification}; isActive={client.IsActive}",
            ip,
            ua,
            ct
        );

        return Result.Success(client.Id);

        async Task<Result<Guid>> Fail(string reason, Error error)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "Client",
                null,
                "CLIENT_CREATE_FAILED",
                string.Empty,
                $"reason={reason}; identification={identification}; email={email}; tradeName={command.TradeName}; primaryPhone={command.PrimaryPhone}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(error);
        }
    }
}
