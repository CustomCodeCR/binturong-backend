using SharedKernel;

namespace Domain.AuditLogs;

public static class AuditLogErrors
{
    public static Error NotFound(Guid auditId) =>
        Error.NotFound(
            "AuditLog.NotFound",
            $"The audit log with the Id = '{auditId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("AuditLog.Unauthorized", "You are not authorized to perform this action.");
}
