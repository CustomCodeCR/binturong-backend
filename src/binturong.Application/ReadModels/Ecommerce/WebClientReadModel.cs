namespace Application.ReadModels.Ecommerce;

public sealed class WebClientReadModel
{
    public string Id { get; init; } = default!; // "web_client:{WebClientId}"
    public int WebClientId { get; init; }

    public int? ClientId { get; init; }
    public string LoginEmail { get; init; } = default!;
    public bool IsActive { get; init; }
}
