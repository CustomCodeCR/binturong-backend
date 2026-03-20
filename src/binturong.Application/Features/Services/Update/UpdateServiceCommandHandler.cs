using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Services.Update;

internal sealed class UpdateServiceCommandHandler : ICommandHandler<UpdateServiceCommand>
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

    public UpdateServiceCommandHandler(
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

    public async Task<Result> Handle(UpdateServiceCommand cmd, CancellationToken ct)
    {
        var service = await _db.Services.FirstOrDefaultAsync(x => x.Id == cmd.ServiceId, ct);
        if (service is null)
            return Result.Failure(ServiceErrors.NotFound(cmd.ServiceId));

        if (string.IsNullOrWhiteSpace(cmd.Code))
            return Result.Failure(ServiceErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result.Failure(ServiceErrors.NameRequired);

        if (cmd.CategoryId == Guid.Empty)
            return Result.Failure(ServiceErrors.CategoryRequired);

        if (cmd.BaseRate <= 0)
            return Result.Failure(ServiceErrors.BaseRateInvalid);

        if (cmd.StandardTimeMin <= 0)
            return Result.Failure(ServiceErrors.StandardTimeInvalid);

        var availability = cmd.AvailabilityStatus.Trim();
        if (!AllowedAvailabilityStatuses.Contains(availability, StringComparer.OrdinalIgnoreCase))
            return Result.Failure(ServiceErrors.AvailabilityStatusInvalid);

        var duplicated = await _db.Services.AnyAsync(
            x => x.Id != cmd.ServiceId && x.Name.ToLower() == cmd.Name.Trim().ToLower(),
            ct
        );

        if (duplicated)
            return Result.Failure(ServiceErrors.NameDuplicated);

        if (service.IsCategoryProtected && service.CategoryId != cmd.CategoryId)
            return Result.Failure(ServiceErrors.CategoryProtected);

        var category = await _db.ProductCategories.FirstOrDefaultAsync(
            x => x.Id == cmd.CategoryId,
            ct
        );
        if (category is null)
            return Result.Failure(ServiceErrors.CategoryNotFound(cmd.CategoryId));

        service.Code = cmd.Code.Trim();
        service.Name = cmd.Name.Trim();
        service.Description = cmd.Description?.Trim() ?? string.Empty;
        service.CategoryId = category.Id;
        service.CategoryName = category.Name;
        service.IsCategoryProtected = cmd.IsCategoryProtected;
        service.StandardTimeMin = cmd.StandardTimeMin;
        service.BaseRate = cmd.BaseRate;
        service.IsActive = cmd.IsActive;
        service.AvailabilityStatus = availability;

        service.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Services",
            "Service",
            service.Id,
            "SERVICE_UPDATED",
            string.Empty,
            $"code={service.Code}; name={service.Name}; categoryId={service.CategoryId}; time={service.StandardTimeMin}; active={service.IsActive}; availability={service.AvailabilityStatus}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
