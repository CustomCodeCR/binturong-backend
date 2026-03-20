using Application.Abstractions.Messaging;
using Application.ReadModels.Services;

namespace Application.Features.Employees.GetWorkHistory;

public sealed record GetEmployeeWorkHistoryQuery(Guid EmployeeId)
    : IQuery<EmployeeWorkHistoryReadModel>;
