using SharedKernel;

namespace Domain.ShoppingCarts;

public static class ShoppingCartErrors
{
    public static Error NotFound(Guid cartId) =>
        Error.NotFound(
            "ShoppingCarts.NotFound",
            $"The shopping cart with the Id = '{cartId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "ShoppingCarts.Unauthorized",
            "You are not authorized to perform this action."
        );
}
