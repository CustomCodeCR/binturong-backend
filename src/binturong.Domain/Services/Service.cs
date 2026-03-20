using Domain.ProductCategories;
using SharedKernel;

namespace Domain.Services;

public sealed class Service : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsCategoryProtected { get; set; }

    public int StandardTimeMin { get; set; }
    public decimal BaseRate { get; set; }
    public bool IsActive { get; set; }

    // Active | Inactive | Maintenance
    public string AvailabilityStatus { get; set; } = "Active";

    public ProductCategory? Category { get; set; }

    public ICollection<Domain.ServiceOrderServices.ServiceOrderService> ServiceOrderServices { get; set; } =
        new List<Domain.ServiceOrderServices.ServiceOrderService>();

    public void RaiseCreated() =>
        Raise(
            new ServiceCreatedDomainEvent(
                Id,
                Code,
                Name,
                Description,
                CategoryId,
                CategoryName,
                IsCategoryProtected,
                StandardTimeMin,
                BaseRate,
                IsActive,
                AvailabilityStatus
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new ServiceUpdatedDomainEvent(
                Id,
                Code,
                Name,
                Description,
                CategoryId,
                CategoryName,
                IsCategoryProtected,
                StandardTimeMin,
                BaseRate,
                IsActive,
                AvailabilityStatus,
                DateTime.UtcNow
            )
        );

    public void RaiseDeleted() => Raise(new ServiceDeletedDomainEvent(Id));

    public Result ValidateAvailability()
    {
        if (!IsActive)
            return Result.Failure(ServiceErrors.NotAvailable);

        if (!string.Equals(AvailabilityStatus, "Active", StringComparison.OrdinalIgnoreCase))
            return Result.Failure(ServiceErrors.NotAvailable);

        return Result.Success();
    }
}
