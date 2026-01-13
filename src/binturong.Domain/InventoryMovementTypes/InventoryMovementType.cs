using SharedKernel;

namespace Domain.InventoryMovementTypes;

public sealed class InventoryMovementType : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Sign { get; set; }

    public ICollection<Domain.InventoryMovements.InventoryMovement> Movements { get; set; } =
        new List<Domain.InventoryMovements.InventoryMovement>();
}
