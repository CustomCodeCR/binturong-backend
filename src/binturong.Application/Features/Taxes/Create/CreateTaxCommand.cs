using Application.Abstractions.Messaging;

namespace Application.Features.Taxes.Create;

public sealed record CreateTaxCommand(
    string Name,
    string Code,
    decimal Percentage,
    bool IsActive = true
) : ICommand<Guid>;
