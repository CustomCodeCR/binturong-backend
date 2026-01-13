namespace Application.ReadModels.Ecommerce;

public sealed class ShoppingCartReadModel
{
    public string Id { get; init; } = default!; // "cart:{CartId}"
    public int CartId { get; init; }

    public int WebClientId { get; init; }
    public string Status { get; init; } = default!;

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<CartItemReadModel> Items { get; init; } = [];

    public decimal Subtotal { get; init; }
    public decimal Taxes { get; init; }
    public decimal Total { get; init; }
}

public sealed class CartItemReadModel
{
    public int CartItemId { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}
