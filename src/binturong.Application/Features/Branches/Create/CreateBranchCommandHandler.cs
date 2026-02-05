using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Branches;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Branches.Create;

internal sealed class CreateBranchCommandHandler : ICommandHandler<CreateBranchCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateBranchCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateBranchCommand command, CancellationToken ct)
    {
        var code = command.Code.Trim();
        var name = command.Name.Trim();
        var address = command.Address.Trim();
        var phone = command.Phone.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Guid>(Domain.Branches.BranchErrors.CodeIsRequired);
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Guid>(Domain.Branches.BranchErrors.NameIsRequired);
        if (string.IsNullOrWhiteSpace(address))
            return Result.Failure<Guid>(Domain.Branches.BranchErrors.AddressIsRequired);
        if (string.IsNullOrWhiteSpace(phone))
            return Result.Failure<Guid>(Domain.Branches.BranchErrors.PhoneIsRequired);

        var codeExists = await _db.Branches.AnyAsync(x => x.Code.ToLower() == code.ToLower(), ct);
        if (codeExists)
            return Result.Failure<Guid>(Domain.Branches.BranchErrors.CodeNotUnique);

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

        return Result.Success(branch.Id);
    }
}
