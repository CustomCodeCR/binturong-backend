namespace Application.ReadModels.Common;

public sealed class MoneyReadModel
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = default!;
    public decimal? ExchangeRate { get; init; }
}
