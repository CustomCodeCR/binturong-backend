using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.UnitsOfMeasure.GetUnitOfMeasureById;

public sealed record GetUnitOfMeasureByIdQuery(Guid UomId) : IQuery<UnitOfMeasureReadModel>;
