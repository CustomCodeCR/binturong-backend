using SharedKernel;

namespace Domain.ServiceOrderMaterials;

public static class ServiceOrderMaterialErrors
{
    public static Error NotFound(Guid materialId) =>
        Error.NotFound(
            "ServiceOrderMaterials.NotFound",
            $"The service order material with the Id = '{materialId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ServiceOrderMaterials.Unauthorized",
            "You are not authorized to perform this action."
        );
}
