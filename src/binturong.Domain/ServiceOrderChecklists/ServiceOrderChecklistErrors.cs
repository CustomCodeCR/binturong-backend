using SharedKernel;

namespace Domain.ServiceOrderChecklists;

public static class ServiceOrderChecklistErrors
{
    public static Error NotFound(Guid checklistId) =>
        Error.NotFound(
            "ServiceOrderChecklists.NotFound",
            $"The checklist item with the Id = '{checklistId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ServiceOrderChecklists.Unauthorized",
            "You are not authorized to perform this action."
        );
}
