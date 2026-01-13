using SharedKernel;

namespace Domain.OutboxMessages;

public static class OutboxMessageErrors
{
    public static Error NotFound(Guid outboxId) =>
        Error.NotFound(
            "OutboxMessages.NotFound",
            $"The outbox message with the Id = '{outboxId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "OutboxMessages.Unauthorized",
            "You are not authorized to perform this action."
        );
}
