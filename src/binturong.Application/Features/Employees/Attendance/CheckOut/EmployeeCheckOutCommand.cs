using Application.Abstractions.Messaging;

namespace Application.Features.Employees.Attendance.CheckOut;

public sealed record EmployeeCheckOutCommand(Guid EmployeeId) : ICommand;
