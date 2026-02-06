using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Branches.Update;

internal sealed class UpdateBranchCommandHandler : ICommandHandler<UpdateBranchCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateBranchCommandHandler(
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

    public async Task<Result> Handle(UpdateBranchCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId; // ajustá si tu propiedad se llama diferente

        var branch = await _db.Branches.FirstOrDefaultAsync(x => x.Id == command.BranchId, ct);
        if (branch is null)
        {
            await _bus.AuditAsync(
                userId,
                "Branches",
                "Branch",
                command.BranchId,
                "BRANCH_UPDATE_FAILED",
                string.Empty,
                $"reason=not_found; branchId={command.BranchId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(Domain.Branches.BranchErrors.NotFound(command.BranchId));
        }

        // BEFORE snapshot
        var before =
            $"code={branch.Code}; name={branch.Name}; address={branch.Address}; phone={branch.Phone}; isActive={branch.IsActive}";

        var code = command.Code.Trim();
        var name = command.Name.Trim();
        var address = command.Address.Trim();
        var phone = command.Phone.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return await Fail("code_required");
        if (string.IsNullOrWhiteSpace(name))
            return await Fail("name_required");
        if (string.IsNullOrWhiteSpace(address))
            return await Fail("address_required");
        if (string.IsNullOrWhiteSpace(phone))
            return await Fail("phone_required");

        var codeExists = await _db.Branches.AnyAsync(
            x => x.Id != command.BranchId && x.Code.ToLower() == code.ToLower(),
            ct
        );

        if (codeExists)
            return await Fail("code_not_unique");

        branch.Code = code;
        branch.Name = name;
        branch.Address = address;
        branch.Phone = phone;
        branch.IsActive = command.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;

        branch.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        var after =
            $"code={branch.Code}; name={branch.Name}; address={branch.Address}; phone={branch.Phone}; isActive={branch.IsActive}";

        await _bus.AuditAsync(
            userId,
            "Branches",
            "Branch",
            branch.Id,
            "BRANCH_UPDATED",
            before,
            after,
            ip,
            ua,
            ct
        );

        return Result.Success();

        async Task<Result> Fail(string reason)
        {
            await _bus.AuditAsync(
                userId,
                "Branches",
                "Branch",
                command.BranchId,
                "BRANCH_UPDATE_FAILED",
                before,
                $"reason={reason}; code={code}; name={name}; address={address}; phone={phone}; isActive={command.IsActive}",
                ip,
                ua,
                ct
            );

            return reason switch
            {
                "code_required" => Result.Failure(Domain.Branches.BranchErrors.CodeIsRequired),
                "name_required" => Result.Failure(Domain.Branches.BranchErrors.NameIsRequired),
                "address_required" => Result.Failure(
                    Domain.Branches.BranchErrors.AddressIsRequired
                ),
                "phone_required" => Result.Failure(Domain.Branches.BranchErrors.PhoneIsRequired),
                "code_not_unique" => Result.Failure(Domain.Branches.BranchErrors.CodeNotUnique),
                _ => Result.Failure(Error.Failure("Branches.UpdateFailed", "Update failed")),
            };
        }
    }
}
