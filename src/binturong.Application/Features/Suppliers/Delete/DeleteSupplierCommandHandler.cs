using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Delete;

internal sealed class DeleteSupplierCommandHandler : ICommandHandler<DeleteSupplierCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteSupplierCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteSupplierCommand command, CancellationToken ct)
    {
        var client = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == command.SupplierId, ct);
        if (client is null)
            return Result.Failure(SupplierErrors.NotFound(command.SupplierId));

        client.RaiseDeleted();

        _db.Suppliers.Remove(client);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
