using Application.Abstractions.Messaging;

namespace Application.Features.Employees.Delete;

public sealed record DeleteEmployeeCommand(Guid EmployeeId) : ICommand;
