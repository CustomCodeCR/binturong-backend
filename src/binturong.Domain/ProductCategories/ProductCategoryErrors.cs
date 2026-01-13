using SharedKernel;

namespace Domain.ProductCategories;

public static class ProductCategoryErrors
{
    public static Error NotFound(Guid categoryId) =>
        Error.NotFound(
            "ProductCategories.NotFound",
            $"The product category with the Id = '{categoryId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ProductCategories.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error NameNotUnique = Error.Conflict(
        "ProductCategories.NameNotUnique",
        "The provided category name is not unique"
    );
}
