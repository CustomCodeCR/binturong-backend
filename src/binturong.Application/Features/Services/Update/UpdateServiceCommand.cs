using Application.Abstractions.Messaging;

namespace Application.Features.Services.Update;

public sealed record UpdateServiceCommand(
    Guid ServiceId,
    string Code,
    string Name,
    string Description,
    Guid CategoryId,
    bool IsCategoryProtected,
    int StandardTimeMin,
    decimal BaseRate,
    bool IsActive,
    string AvailabilityStatus
) : ICommand;
