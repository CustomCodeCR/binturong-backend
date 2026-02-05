using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Branches.Delete;

internal sealed class DeleteBranchCommandHandler : ICommandHandler<DeleteBranchCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteBranchCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteBranchCommand command, CancellationToken ct)
    {
        var branch = await _db.Branches.FirstOrDefaultAsync(x => x.Id == command.BranchId, ct);
        if (branch is null)
            return Result.Failure(Domain.Branches.BranchErrors.NotFound(command.BranchId));

        branch.RaiseDeleted();

        _db.Branches.Remove(branch);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
