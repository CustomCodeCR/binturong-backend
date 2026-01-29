using Application.Abstractions.Messaging;

namespace Application.Features.UnitsOfMeasure.Update;

public sealed record UpdateUnitOfMeasureCommand(Guid UomId, string Code, string Name, bool IsActive)
    : ICommand;
