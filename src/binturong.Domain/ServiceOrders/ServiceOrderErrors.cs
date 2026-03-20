using SharedKernel;

namespace Domain.ServiceOrders;

public static class ServiceOrderErrors
{
    public static readonly Error ClientRequired = Error.Validation(
        "ServiceOrders.ClientRequired",
        "ClientId is required."
    );

    public static readonly Error ScheduledDateRequired = Error.Validation(
        "ServiceOrders.ScheduledDateRequired",
        "ScheduledDate is required."
    );

    public static readonly Error ServiceAddressRequired = Error.Validation(
        "ServiceOrders.ServiceAddressRequired",
        "ServiceAddress is required."
    );

    public static readonly Error PendingStatusRequiredForAssignment = Error.Validation(
        "ServiceOrders.PendingStatusRequiredForAssignment",
        "Technician can only be assigned when service order is pending."
    );

    public static readonly Error MissingOrderDetails = Error.Validation(
        "ServiceOrders.MissingOrderDetails",
        "Service order must contain address and notes before technician assignment."
    );

    public static readonly Error ServiceRequired = Error.Validation(
        "ServiceOrders.ServiceRequired",
        "At least one service is required."
    );

    public static readonly Error TechnicianAlreadyAssigned = Error.Validation(
        "ServiceOrders.TechnicianAlreadyAssigned",
        "Technician is already assigned to this service order."
    );

    public static readonly Error StatusInvalid = Error.Validation(
        "ServiceOrders.StatusInvalid",
        "Status is invalid."
    );

    public static Error NotFound(Guid id) =>
        Error.NotFound("ServiceOrders.NotFound", $"Service order '{id}' not found.");
}
