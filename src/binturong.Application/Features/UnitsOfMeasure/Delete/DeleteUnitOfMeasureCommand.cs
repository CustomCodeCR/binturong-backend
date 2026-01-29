using Application.Abstractions.Messaging;

namespace Application.Features.UnitsOfMeasure.Delete;

public sealed record DeleteUnitOfMeasureCommand(Guid UomId) : ICommand;
