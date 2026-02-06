using Application.Abstractions.Messaging;
using Application.ReadModels.Audit;

namespace Application.Features.Audit.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    DateTime? From,
    DateTime? To,
    Guid? UserId,
    string? Module,
    string? Action
) : IQuery<IReadOnlyList<AuditLogReadModel>>;
