using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Branches.Delete;

internal sealed class DeleteBranchCommandHandler : ICommandHandler<DeleteBranchCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteBranchCommandHandler(
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

    public async Task<Result> Handle(DeleteBranchCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var branch = await _db.Branches.FirstOrDefaultAsync(x => x.Id == command.BranchId, ct);
        if (branch is null)
        {
            await _bus.AuditAsync(
                userId,
                "Branches",
                "Branch",
                command.BranchId,
                "BRANCH_DELETE_FAILED",
                string.Empty,
                $"reason=not_found; branchId={command.BranchId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(Domain.Branches.BranchErrors.NotFound(command.BranchId));
        }

        var before = $"code={branch.Code}; name={branch.Name}; isActive={branch.IsActive}";

        branch.RaiseDeleted();

        _db.Branches.Remove(branch);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Branches",
            "Branch",
            command.BranchId,
            "BRANCH_DELETED",
            before,
            $"branchId={command.BranchId}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
