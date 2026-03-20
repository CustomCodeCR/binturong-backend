using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Services.Create;

internal sealed class CreateServiceCommandHandler : ICommandHandler<CreateServiceCommand, Guid>
{
    private static readonly string[] AllowedAvailabilityStatuses =
    [
        "Active",
        "Inactive",
        "Maintenance",
    ];

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateServiceCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateServiceCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.Code))
            return Result.Failure<Guid>(ServiceErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result.Failure<Guid>(ServiceErrors.NameRequired);

        if (cmd.CategoryId == Guid.Empty)
            return Result.Failure<Guid>(ServiceErrors.CategoryRequired);

        if (cmd.BaseRate <= 0)
            return Result.Failure<Guid>(ServiceErrors.BaseRateInvalid);

        if (cmd.StandardTimeMin <= 0)
            return Result.Failure<Guid>(ServiceErrors.StandardTimeInvalid);

        var availability = cmd.AvailabilityStatus.Trim();
        if (!AllowedAvailabilityStatuses.Contains(availability, StringComparer.OrdinalIgnoreCase))
            return Result.Failure<Guid>(ServiceErrors.AvailabilityStatusInvalid);

        var exists = await _db.Services.AnyAsync(
            x => x.Name.ToLower() == cmd.Name.Trim().ToLower(),
            ct
        );

        if (exists)
            return Result.Failure<Guid>(ServiceErrors.NameDuplicated);

        var category = await _db.ProductCategories.FirstOrDefaultAsync(
            x => x.Id == cmd.CategoryId,
            ct
        );
        if (category is null)
            return Result.Failure<Guid>(ServiceErrors.CategoryNotFound(cmd.CategoryId));

        var service = new Domain.Services.Service
        {
            Id = Guid.NewGuid(),
            Code = cmd.Code.Trim(),
            Name = cmd.Name.Trim(),
            Description = cmd.Description?.Trim() ?? string.Empty,
            CategoryId = category.Id,
            CategoryName = category.Name,
            IsCategoryProtected = cmd.IsCategoryProtected,
            StandardTimeMin = cmd.StandardTimeMin,
            BaseRate = cmd.BaseRate,
            IsActive = cmd.IsActive,
            AvailabilityStatus = availability,
        };

        service.RaiseCreated();

        _db.Services.Add(service);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Services",
            "Service",
            service.Id,
            "SERVICE_CREATED",
            string.Empty,
            $"code={service.Code}; name={service.Name}; categoryId={service.CategoryId}; baseRate={service.BaseRate}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(service.Id);
    }
}
