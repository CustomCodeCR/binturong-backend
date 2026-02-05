using Application.Abstractions.Messaging;

namespace Application.Features.Employees.Attendance.CheckIn;

public sealed record EmployeeCheckInCommand(Guid EmployeeId) : ICommand;
