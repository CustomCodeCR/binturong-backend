using SharedKernel;

namespace Domain.PurchaseRequests;

public sealed class PurchaseRequest : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public Guid? RequestedById { get; set; }
    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public Domain.Branches.Branch? Branch { get; set; }
    public Domain.Employees.Employee? RequestedBy { get; set; }

    public ICollection<Domain.PurchaseOrders.PurchaseOrder> PurchaseOrders { get; set; } =
        new List<Domain.PurchaseOrders.PurchaseOrder>();
}
