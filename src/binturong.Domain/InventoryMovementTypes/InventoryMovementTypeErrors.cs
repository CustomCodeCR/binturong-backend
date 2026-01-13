using SharedKernel;

namespace Domain.InventoryMovementTypes;

public static class InventoryMovementTypeErrors
{
    public static Error NotFound(Guid movementTypeId) =>
        Error.NotFound(
            "InventoryMovementTypes.NotFound",
            $"The movement type with the Id = '{movementTypeId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "InventoryMovementTypes.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error CodeNotUnique = Error.Conflict(
        "InventoryMovementTypes.CodeNotUnique",
        "The provided movement type code is not unique"
    );

    public static readonly Error InvalidSign = Error.Validation(
        "InventoryMovementTypes.InvalidSign",
        "The movement type sign must be +1 or -1"
    );
}
