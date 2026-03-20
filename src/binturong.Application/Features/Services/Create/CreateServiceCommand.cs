using Application.Abstractions.Messaging;

namespace Application.Features.Services.Create;

public sealed record CreateServiceCommand(
    string Code,
    string Name,
    string Description,
    Guid CategoryId,
    bool IsCategoryProtected,
    int StandardTimeMin,
    decimal BaseRate,
    bool IsActive,
    string AvailabilityStatus
) : ICommand<Guid>;
