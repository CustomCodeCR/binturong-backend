using SharedKernel;

namespace Domain.CartItems;

public sealed record CartItemAddedDomainEvent(Guid CartItemId) : IDomainEvent;
