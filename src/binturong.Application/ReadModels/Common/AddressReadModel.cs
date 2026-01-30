namespace Application.ReadModels.Common;

public sealed class AddressReadModel
{
    public string Id { get; init; } = default!;
    public Guid AddressId { get; init; }

    public string AddressType { get; init; } = default!;
    public string AddressLine { get; init; } = default!;
    public string Province { get; init; } = default!;
    public string Canton { get; init; } = default!;
    public string District { get; init; } = default!;
    public string? Notes { get; init; }
    public bool IsPrimary { get; init; }
}
