using Application.Abstractions.Messaging;

namespace Application.Features.ServiceOrders.AssignTechnician;

public sealed record AssignServiceOrderTechnicianCommand(
    Guid ServiceOrderId,
    Guid EmployeeId,
    string TechRole
) : ICommand;
