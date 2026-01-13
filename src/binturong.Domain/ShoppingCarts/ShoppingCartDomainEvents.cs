using SharedKernel;

namespace Domain.ShoppingCarts;

public sealed record ShoppingCartCreatedDomainEvent(Guid CartId) : IDomainEvent;
