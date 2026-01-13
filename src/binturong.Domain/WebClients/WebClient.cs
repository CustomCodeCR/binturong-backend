using SharedKernel;

namespace Domain.WebClients;

public sealed class WebClient : Entity
{
    public Guid Id { get; set; }
    public Guid? ClientId { get; set; }
    public string LoginEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public Domain.Clients.Client? Client { get; set; }
    public ICollection<Domain.ShoppingCarts.ShoppingCart> ShoppingCarts { get; set; } =
        new List<Domain.ShoppingCarts.ShoppingCart>();

    public ICollection<Domain.MarketingAssetTracking.MarketingAssetTrackingEntry> MarketingTrackings { get; set; } =
        new List<Domain.MarketingAssetTracking.MarketingAssetTrackingEntry>();
}
