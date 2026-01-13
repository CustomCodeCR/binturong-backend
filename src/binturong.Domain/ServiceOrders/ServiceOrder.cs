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
    public string Status { get; set; } = string.Empty;
    public string ServiceAddress { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public Domain.Clients.Client? Client { get; set; }
    public Domain.Branches.Branch? Branch { get; set; }
    public Domain.Contracts.Contract? Contract { get; set; }

    public ICollection<Domain.ServiceOrderTechnicians.ServiceOrderTechnician> Technicians { get; set; } =
        new List<Domain.ServiceOrderTechnicians.ServiceOrderTechnician>();

    public ICollection<Domain.ServiceOrderServices.ServiceOrderService> Services { get; set; } =
        new List<Domain.ServiceOrderServices.ServiceOrderService>();

    public ICollection<Domain.ServiceOrderMaterials.ServiceOrderMaterial> Materials { get; set; } =
        new List<Domain.ServiceOrderMaterials.ServiceOrderMaterial>();

    public ICollection<Domain.ServiceOrderChecklists.ServiceOrderChecklist> Checklists { get; set; } =
        new List<Domain.ServiceOrderChecklists.ServiceOrderChecklist>();

    public ICollection<Domain.ServiceOrderPhotos.ServiceOrderPhoto> Photos { get; set; } =
        new List<Domain.ServiceOrderPhotos.ServiceOrderPhoto>();
}
