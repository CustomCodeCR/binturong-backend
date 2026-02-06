using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Branches;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Branches.Create;

internal sealed class CreateBranchCommandHandler : ICommandHandler<CreateBranchCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateBranchCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateBranchCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var code = command.Code.Trim();
        var name = command.Name.Trim();
        var address = command.Address.Trim();
        var phone = command.Phone.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return await FailGuid("code_required");
        if (string.IsNullOrWhiteSpace(name))
            return await FailGuid("name_required");
        if (string.IsNullOrWhiteSpace(address))
            return await FailGuid("address_required");
        if (string.IsNullOrWhiteSpace(phone))
            return await FailGuid("phone_required");

        var codeExists = await _db.Branches.AnyAsync(x => x.Code.ToLower() == code.ToLower(), ct);
        if (codeExists)
            return await FailGuid("code_not_unique");

        var now = DateTime.UtcNow;

        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Address = address,
            Phone = phone,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        branch.RaiseCreated();

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Branches",
            "Branch",
            branch.Id,
            "BRANCH_CREATED",
            string.Empty,
            $"branchId={branch.Id}; code={branch.Code}; name={branch.Name}; address={branch.Address}; phone={branch.Phone}; isActive={branch.IsActive}",
            ip,
            ua,
            ct
        );

        return Result.Success(branch.Id);

        async Task<Result<Guid>> FailGuid(string reason)
        {
            await _bus.AuditAsync(
                userId,
                "Branches",
                "Branch",
                null,
                "BRANCH_CREATE_FAILED",
                string.Empty,
                $"reason={reason}; code={code}; name={name}; address={address}; phone={phone}; isActive={command.IsActive}",
                ip,
                ua,
                ct
            );

            return reason switch
            {
                "code_required" => Result.Failure<Guid>(
                    Domain.Branches.BranchErrors.CodeIsRequired
                ),
                "name_required" => Result.Failure<Guid>(
                    Domain.Branches.BranchErrors.NameIsRequired
                ),
                "address_required" => Result.Failure<Guid>(
                    Domain.Branches.BranchErrors.AddressIsRequired
                ),
                "phone_required" => Result.Failure<Guid>(
                    Domain.Branches.BranchErrors.PhoneIsRequired
                ),
                "code_not_unique" => Result.Failure<Guid>(
                    Domain.Branches.BranchErrors.CodeNotUnique
                ),
                _ => Result.Failure<Guid>(Error.Failure("Branches.CreateFailed", "Create failed")),
            };
        }
    }
}
