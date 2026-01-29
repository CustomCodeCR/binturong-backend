using Application.Abstractions.Messaging;

namespace Application.Features.UnitsOfMeasure.Create;

public sealed record CreateUnitOfMeasureCommand(string Code, string Name, bool IsActive = true)
    : ICommand<Guid>;
