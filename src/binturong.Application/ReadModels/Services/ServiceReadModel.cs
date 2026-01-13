namespace Application.ReadModels.Services;

public sealed class ServiceReadModel
{
    public string Id { get; init; } = default!; // "service:{ServiceId}"
    public int ServiceId { get; init; }

    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; }

    public int StandardTimeMin { get; init; }
    public decimal BaseRate { get; init; }

    public bool IsActive { get; init; }
}
