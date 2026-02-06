using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Update;

internal sealed class UpdateClientCommandHandler : ICommandHandler<UpdateClientCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateClientCommandHandler(
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

    public async Task<Result> Handle(UpdateClientCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var client = await _db.Clients.FirstOrDefaultAsync(x => x.Id == command.ClientId, ct);
        if (client is null)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "Client",
                command.ClientId,
                "CLIENT_UPDATE_FAILED",
                string.Empty,
                $"reason=not_found; clientId={command.ClientId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientErrors.NotFound(command.ClientId));
        }

        var before =
            $"tradeName={client.TradeName}; contactName={client.ContactName}; email={client.Email}; primaryPhone={client.PrimaryPhone}; secondaryPhone={client.SecondaryPhone}; industry={client.Industry}; clientType={client.ClientType}; score={client.Score}; isActive={client.IsActive}";

        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return await Fail("email_required", ClientErrors.EmailIsRequired);

        if (string.IsNullOrWhiteSpace(command.TradeName))
            return await Fail("trade_name_required", ClientErrors.TradeNameIsRequired);

        if (string.IsNullOrWhiteSpace(command.PrimaryPhone))
            return await Fail("primary_phone_required", ClientErrors.PrimaryPhoneIsRequired);

        var emailExists = await _db.Clients.AnyAsync(
            x => x.Id != command.ClientId && x.Email.ToLower() == email,
            ct
        );
        if (emailExists)
            return await Fail("email_not_unique", ClientErrors.EmailNotUnique);

        client.UpdateBasicInfo(
            tradeName: command.TradeName.Trim(),
            contactName: command.ContactName.Trim(),
            email: email,
            primaryPhone: command.PrimaryPhone.Trim(),
            secondaryPhone: string.IsNullOrWhiteSpace(command.SecondaryPhone)
                ? null
                : command.SecondaryPhone.Trim(),
            industry: string.IsNullOrWhiteSpace(command.Industry) ? null : command.Industry.Trim(),
            clientType: string.IsNullOrWhiteSpace(command.ClientType)
                ? null
                : command.ClientType.Trim(),
            score: command.Score,
            isActive: command.IsActive
        );

        await _db.SaveChangesAsync(ct);

        var after =
            $"tradeName={client.TradeName}; contactName={client.ContactName}; email={client.Email}; primaryPhone={client.PrimaryPhone}; secondaryPhone={client.SecondaryPhone}; industry={client.Industry}; clientType={client.ClientType}; score={client.Score}; isActive={client.IsActive}";

        await _bus.AuditAsync(
            userId,
            "Clients",
            "Client",
            client.Id,
            "CLIENT_UPDATED",
            before,
            after,
            ip,
            ua,
            ct
        );

        return Result.Success();

        async Task<Result> Fail(string reason, Error error)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "Client",
                client.Id,
                "CLIENT_UPDATE_FAILED",
                before,
                $"reason={reason}; email={email}; tradeName={command.TradeName}; primaryPhone={command.PrimaryPhone}; isActive={command.IsActive}",
                ip,
                ua,
                ct
            );

            return Result.Failure(error);
        }
    }
}
