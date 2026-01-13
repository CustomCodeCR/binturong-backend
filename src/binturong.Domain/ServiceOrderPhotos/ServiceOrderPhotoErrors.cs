using SharedKernel;

namespace Domain.ServiceOrderPhotos;

public static class ServiceOrderPhotoErrors
{
    public static Error NotFound(Guid photoId) =>
        Error.NotFound(
            "ServiceOrderPhotos.NotFound",
            $"The service order photo with the Id = '{photoId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ServiceOrderPhotos.Unauthorized",
            "You are not authorized to perform this action."
        );
}
