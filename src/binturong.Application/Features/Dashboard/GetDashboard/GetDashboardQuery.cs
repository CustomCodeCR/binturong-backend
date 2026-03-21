using Application.Abstractions.Messaging;
using Application.ReadModels.Dashboard;

namespace Application.Features.Dashboard.GetDashboard;

public sealed record GetDashboardQuery(Guid? BranchId) : IQuery<DashboardReadModel>;
