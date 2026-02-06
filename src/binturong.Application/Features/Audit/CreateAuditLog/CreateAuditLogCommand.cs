using Application.Abstractions.Messaging;

namespace Application.Features.Audit.Create;

public sealed record CreateAuditLogCommand(
    Guid? UserId,
    string Module,
    string Entity,
    Guid? EntityId,
    string Action,
    string? DataBefore,
    string? DataAfter,
    string? Ip,
    string? UserAgent
) : ICommand;
