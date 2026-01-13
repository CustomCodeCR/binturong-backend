using SharedKernel;

namespace Domain.ShoppingCarts;

public sealed class ShoppingCart : Entity
{
    public Guid Id { get; set; }
    public Guid WebClientId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Status { get; set; } = string.Empty;

    public Domain.WebClients.WebClient? WebClient { get; set; }
    public ICollection<Domain.CartItems.CartItem> Items { get; set; } =
        new List<Domain.CartItems.CartItem>();
}
