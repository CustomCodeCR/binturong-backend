using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Contracts.Delete;

internal sealed class DeleteContractCommandHandler : ICommandHandler<DeleteContractCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteContractCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteContractCommand cmd, CancellationToken ct)
    {
        var contract = await _db
            .Contracts.Include(x => x.BillingMilestones)
            .FirstOrDefaultAsync(x => x.Id == cmd.ContractId, ct);

        if (contract is null)
            return Result.Failure(ContractErrors.NotFound(cmd.ContractId));

        contract.RaiseDeleted();

        _db.Contracts.Remove(contract);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
