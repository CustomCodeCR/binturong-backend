namespace Application.ReadModels.Common;

public sealed class ContactReadModel
{
    public int ContactId { get; init; }
    public string Name { get; init; } = default!;
    public string? JobTitle { get; init; }
    public string Email { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public bool IsPrimary { get; init; }
}
