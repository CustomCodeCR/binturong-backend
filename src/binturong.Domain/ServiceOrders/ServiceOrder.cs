using Domain.ServiceOrderChecklists;
using Domain.ServiceOrderMaterials;
using Domain.ServiceOrderPhotos;
using Domain.ServiceOrderServices;
using Domain.ServiceOrderTechnicians;
using SharedKernel;

namespace Domain.ServiceOrders;

public sealed class ServiceOrder : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ContractId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string ServiceAddress { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public Domain.Clients.Client? Client { get; set; }
    public Domain.Branches.Branch? Branch { get; set; }
    public Domain.Contracts.Contract? Contract { get; set; }

    public ICollection<ServiceOrderTechnician> Technicians { get; set; } =
        new List<ServiceOrderTechnician>();

    public ICollection<ServiceOrderService> Services { get; set; } =
        new List<ServiceOrderService>();

    public ICollection<ServiceOrderMaterial> Materials { get; set; } =
        new List<ServiceOrderMaterial>();

    public ICollection<ServiceOrderChecklist> Checklists { get; set; } =
        new List<ServiceOrderChecklist>();

    public ICollection<ServiceOrderPhoto> Photos { get; set; } = new List<ServiceOrderPhoto>();

    public void RaiseCreated() =>
        Raise(
            new ServiceOrderCreatedDomainEvent(
                Id,
                Code,
                ClientId,
                BranchId,
                ContractId,
                ScheduledDate,
                Status,
                ServiceAddress,
                Notes
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new ServiceOrderUpdatedDomainEvent(
                Id,
                ScheduledDate,
                Status,
                ServiceAddress,
                Notes,
                DateTime.UtcNow
            )
        );

    public void RaiseDeleted() => Raise(new ServiceOrderDeletedDomainEvent(Id));

    public Result CanAssignTechnician()
    {
        if (!string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase))
            return Result.Failure(ServiceOrderErrors.PendingStatusRequiredForAssignment);

        if (string.IsNullOrWhiteSpace(ServiceAddress) || string.IsNullOrWhiteSpace(Notes))
            return Result.Failure(ServiceOrderErrors.MissingOrderDetails);

        return Result.Success();
    }

    public void AddService(ServiceOrderService line, string serviceName)
    {
        Services.Add(line);

        Raise(
            new ServiceOrderServiceAddedDomainEvent(
                Id,
                line.Id,
                line.ServiceId,
                serviceName,
                line.Quantity,
                line.RateApplied,
                line.LineTotal
            )
        );
    }

    public void AssignTechnician(
        ServiceOrderTechnician tech,
        string employeeName,
        DateTime assignedAtUtc
    )
    {
        Technicians.Add(tech);

        Raise(
            new ServiceOrderTechnicianAssignedDomainEvent(
                Id,
                tech.Id,
                tech.EmployeeId,
                employeeName,
                tech.TechRole,
                assignedAtUtc
            )
        );
    }

    public void AddMaterial(ServiceOrderMaterial material, string productName)
    {
        Materials.Add(material);

        Raise(
            new ServiceOrderMaterialAddedDomainEvent(
                Id,
                material.Id,
                material.ProductId,
                productName,
                material.Quantity,
                material.EstimatedCost
            )
        );
    }

    public void AddChecklist(ServiceOrderChecklist checklist)
    {
        Checklists.Add(checklist);

        Raise(
            new ServiceOrderChecklistAddedDomainEvent(
                Id,
                checklist.Id,
                checklist.Description,
                checklist.IsCompleted
            )
        );
    }

    public void AddPhoto(ServiceOrderPhoto photo)
    {
        Photos.Add(photo);

        Raise(
            new ServiceOrderPhotoAddedDomainEvent(Id, photo.Id, photo.PhotoS3Key, photo.Description)
        );
    }

    public void Close(DateTime closedDateUtc)
    {
        ClosedDate = closedDateUtc;
        Status = "Closed";

        Raise(new ServiceOrderClosedDomainEvent(Id, closedDateUtc));
        RaiseUpdated();
    }
}
