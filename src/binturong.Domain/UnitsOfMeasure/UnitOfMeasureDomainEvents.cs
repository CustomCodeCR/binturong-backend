using SharedKernel;

namespace Domain.UnitsOfMeasure;

public sealed record UnitOfMeasureCreatedDomainEvent(Guid UomId) : IDomainEvent;
