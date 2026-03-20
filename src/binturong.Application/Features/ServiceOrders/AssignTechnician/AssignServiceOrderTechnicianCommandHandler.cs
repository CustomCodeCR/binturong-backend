using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ServiceOrders;
using Domain.ServiceOrderTechnicians;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.ServiceOrders.AssignTechnician;

internal sealed class AssignServiceOrderTechnicianCommandHandler
    : ICommandHandler<AssignServiceOrderTechnicianCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public AssignServiceOrderTechnicianCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(AssignServiceOrderTechnicianCommand cmd, CancellationToken ct)
    {
        var order = await _db
            .ServiceOrders.Include(x => x.Technicians)
            .FirstOrDefaultAsync(x => x.Id == cmd.ServiceOrderId, ct);

        if (order is null)
            return Result.Failure(ServiceOrderErrors.NotFound(cmd.ServiceOrderId));

        var canAssign = order.CanAssignTechnician();
        if (canAssign.IsFailure)
            return canAssign;

        if (cmd.EmployeeId == Guid.Empty)
            return Result.Failure(ServiceOrderTechnicianErrors.EmployeeRequired);

        if (string.IsNullOrWhiteSpace(cmd.TechRole))
            return Result.Failure(ServiceOrderTechnicianErrors.TechRoleRequired);

        var employee = await _db.Employees.FirstOrDefaultAsync(x => x.Id == cmd.EmployeeId, ct);
        if (employee is null)
            return Result.Failure(
                Error.NotFound("Employees.NotFound", $"Employee '{cmd.EmployeeId}' not found.")
            );

        if (!employee.IsActive)
            return Result.Failure(ServiceOrderTechnicianErrors.EmployeeInactive);

        var duplicated = order.Technicians.Any(x => x.EmployeeId == cmd.EmployeeId);
        if (duplicated)
            return Result.Failure(ServiceOrderErrors.TechnicianAlreadyAssigned);

        var isBusy = await _db
            .ServiceOrderTechnicians.Include(x => x.ServiceOrder)
            .AnyAsync(
                x =>
                    x.EmployeeId == cmd.EmployeeId
                    && x.ServiceOrderId != cmd.ServiceOrderId
                    && x.ServiceOrder != null
                    && x.ServiceOrder.ScheduledDate == order.ScheduledDate
                    && x.ServiceOrder.Status != "Closed",
                ct
            );

        if (isBusy)
            return Result.Failure(ServiceOrderTechnicianErrors.TechnicianBusy);

        var tech = new ServiceOrderTechnician
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = order.Id,
            EmployeeId = employee.Id,
            TechRole = cmd.TechRole.Trim(),
        };

        order.AssignTechnician(tech, employee.FullName, DateTime.UtcNow);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "ServiceOrders",
            "ServiceOrder",
            order.Id,
            "SERVICE_ORDER_TECHNICIAN_ASSIGNED",
            string.Empty,
            $"employeeId={employee.Id}; employeeName={employee.FullName}; role={tech.TechRole}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
