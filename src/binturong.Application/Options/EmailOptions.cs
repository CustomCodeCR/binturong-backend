namespace Application.Options;

public sealed class EmailOptions
{
    public string? Host { get; init; }
    public int Port { get; init; } = 587;
    public bool UseSsl { get; init; } = true;
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string FromEmail { get; init; } = "noreply@binturong.local";
    public string FromName { get; init; } = "Binturong";

    public string? AdminEmail { get; init; }
    public string? PurchasesEmail { get; init; }
    public string? ContractsEmail { get; init; }
}
