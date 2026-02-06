using SharedKernel;

namespace Domain.AuditLogs;

public static class AuditLogErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Audit.NotFound",
        "Audit log entry was not found."
    );

    public static readonly Error Forbidden = Error.Failure(
        "Audit.Forbidden",
        "You are not allowed to access audit logs."
    );
}
