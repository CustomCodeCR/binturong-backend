using SharedKernel;

namespace Domain.ContractBillingMilestones;

public static class ContractBillingMilestoneErrors
{
    public static Error NotFound(Guid milestoneId) =>
        Error.NotFound(
            "ContractBillingMilestones.NotFound",
            $"The billing milestone with the Id = '{milestoneId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ContractBillingMilestones.Unauthorized",
            "You are not authorized to perform this action."
        );
}
