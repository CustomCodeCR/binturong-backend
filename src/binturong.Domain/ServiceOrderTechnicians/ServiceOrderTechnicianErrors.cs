using SharedKernel;

namespace Domain.ServiceOrderTechnicians;

public static class ServiceOrderTechnicianErrors
{
    public static readonly Error EmployeeRequired = Error.Validation(
        "ServiceOrderTechnicians.EmployeeRequired",
        "EmployeeId is required."
    );

    public static readonly Error TechRoleRequired = Error.Validation(
        "ServiceOrderTechnicians.TechRoleRequired",
        "TechRole is required."
    );

    public static readonly Error EmployeeInactive = Error.Validation(
        "ServiceOrderTechnicians.EmployeeInactive",
        "Employee is inactive."
    );

    public static readonly Error TechnicianBusy = Error.Validation(
        "ServiceOrderTechnicians.TechnicianBusy",
        "Technician is already assigned to another service order in the same schedule."
    );
}
