using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Delete;

internal sealed class DeleteClientCommandHandler : ICommandHandler<DeleteClientCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteClientCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteClientCommand command, CancellationToken ct)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(x => x.Id == command.ClientId, ct);
        if (client is null)
            return Result.Failure(ClientErrors.NotFound(command.ClientId));

        client.RaiseDeleted();

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
