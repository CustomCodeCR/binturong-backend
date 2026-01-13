using SharedKernel;

namespace Domain.ServiceOrderTechnicians;

public static class ServiceOrderTechnicianErrors
{
    public static Error NotFound(Guid techId) =>
        Error.NotFound(
            "ServiceOrderTechnicians.NotFound",
            $"The service order technician with the Id = '{techId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ServiceOrderTechnicians.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error DuplicateTechnicianAssignment = Error.Conflict(
        "ServiceOrderTechnicians.DuplicateTechnicianAssignment",
        "This technician is already assigned to the service order"
    );
}
