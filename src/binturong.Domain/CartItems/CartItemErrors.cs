using SharedKernel;

namespace Domain.CartItems;

public static class CartItemErrors
{
    public static Error NotFound(Guid cartItemId) =>
        Error.NotFound(
            "CartItems.NotFound",
            $"The cart item with the Id = '{cartItemId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("CartItems.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error DuplicateProductInCart = Error.Conflict(
        "CartItems.DuplicateProductInCart",
        "The product already exists in the cart"
    );
}
