using SharedKernel;

namespace Domain.CartItems;

public sealed class CartItem : Entity
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Domain.ShoppingCarts.ShoppingCart? Cart { get; set; }
    public Domain.Products.Product? Product { get; set; }
}
