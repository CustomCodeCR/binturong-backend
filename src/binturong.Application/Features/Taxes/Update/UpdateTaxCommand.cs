using Application.Abstractions.Messaging;

namespace Application.Features.Taxes.Update;

public sealed record UpdateTaxCommand(
    Guid TaxId,
    string Name,
    string Code,
    decimal Percentage,
    bool IsActive
) : ICommand;
