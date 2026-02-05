using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Branches.Update;

internal sealed class UpdateBranchCommandHandler : ICommandHandler<UpdateBranchCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateBranchCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateBranchCommand command, CancellationToken ct)
    {
        var branch = await _db.Branches.FirstOrDefaultAsync(x => x.Id == command.BranchId, ct);
        if (branch is null)
            return Result.Failure(Domain.Branches.BranchErrors.NotFound(command.BranchId));

        var code = command.Code.Trim();
        var name = command.Name.Trim();
        var address = command.Address.Trim();
        var phone = command.Phone.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(Domain.Branches.BranchErrors.CodeIsRequired);
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Domain.Branches.BranchErrors.NameIsRequired);
        if (string.IsNullOrWhiteSpace(address))
            return Result.Failure(Domain.Branches.BranchErrors.AddressIsRequired);
        if (string.IsNullOrWhiteSpace(phone))
            return Result.Failure(Domain.Branches.BranchErrors.PhoneIsRequired);

        var codeExists = await _db.Branches.AnyAsync(
            x => x.Id != command.BranchId && x.Code.ToLower() == code.ToLower(),
            ct
        );

        if (codeExists)
            return Result.Failure(Domain.Branches.BranchErrors.CodeNotUnique);

        branch.Code = code;
        branch.Name = name;
        branch.Address = address;
        branch.Phone = phone;
        branch.IsActive = command.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;

        branch.RaiseUpdated();

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
