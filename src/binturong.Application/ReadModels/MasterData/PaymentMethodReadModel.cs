namespace Application.ReadModels.MasterData;

public sealed class PaymentMethodReadModel
{
    public string Id { get; init; } = default!; // "pm:{PaymentMethodId}"
    public Guid PaymentMethodId { get; init; }

    public string Code { get; init; } = default!;
    public string Description { get; init; } = default!;
    public bool IsActive { get; init; }

    public DateTime UpdatedAt { get; init; }
}
