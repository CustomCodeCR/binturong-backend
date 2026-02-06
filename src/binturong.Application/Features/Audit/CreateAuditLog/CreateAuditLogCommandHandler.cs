using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.AuditLogs;
using SharedKernel;

namespace Application.Features.Audit.Create;

internal sealed class CreateAuditLogCommandHandler : ICommandHandler<CreateAuditLogCommand>
{
    private readonly IApplicationDbContext _db;

    public CreateAuditLogCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(CreateAuditLogCommand cmd, CancellationToken ct)
    {
        var log = AuditLog.Create(
            cmd.UserId,
            cmd.Module,
            cmd.Entity,
            cmd.EntityId,
            cmd.Action,
            cmd.DataBefore,
            cmd.DataAfter,
            cmd.Ip,
            cmd.UserAgent
        );

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
