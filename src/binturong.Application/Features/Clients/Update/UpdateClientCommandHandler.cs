using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Update;

internal sealed class UpdateClientCommandHandler : ICommandHandler<UpdateClientCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateClientCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateClientCommand command, CancellationToken ct)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(x => x.Id == command.ClientId, ct);
        if (client is null)
            return Result.Failure(ClientErrors.NotFound(command.ClientId));

        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(ClientErrors.EmailIsRequired);

        if (string.IsNullOrWhiteSpace(command.TradeName))
            return Result.Failure(ClientErrors.TradeNameIsRequired);

        if (string.IsNullOrWhiteSpace(command.PrimaryPhone))
            return Result.Failure(ClientErrors.PrimaryPhoneIsRequired);

        // Email unique excluding self
        var emailExists = await _db.Clients.AnyAsync(
            x => x.Id != command.ClientId && x.Email.ToLower() == email,
            ct
        );
        if (emailExists)
            return Result.Failure(ClientErrors.EmailNotUnique);

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

        return Result.Success();
    }
}
