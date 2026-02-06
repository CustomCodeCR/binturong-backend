using Application.Abstractions.Messaging;
using Application.Features.Audit.Create;
using SharedKernel;

namespace Application.Features.Common.Audit;

internal static class AuditExtensions
{
    public static Task<Result> AuditAsync(
        this ICommandBus bus,
        Guid? userId,
        string module,
        string entity,
        Guid? entityId,
        string action,
        string dataBefore,
        string dataAfter,
        string ip,
        string userAgent,
        CancellationToken ct
    ) =>
        bus.Send(
            new CreateAuditLogCommand(
                userId,
                module,
                entity,
                entityId,
                action,
                dataBefore,
                dataAfter,
                ip,
                userAgent
            ),
            ct
        );
}
