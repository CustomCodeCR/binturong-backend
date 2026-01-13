using SharedKernel;

namespace Domain.ProductCategories;

public sealed record ProductCategoryCreatedDomainEvent(Guid CategoryId) : IDomainEvent;
