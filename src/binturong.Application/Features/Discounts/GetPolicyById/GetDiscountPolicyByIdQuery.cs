using Application.Abstractions.Messaging;
using Application.ReadModels.Discounts;

namespace Application.Features.Discounts.GetPolicyById;

public sealed record GetDiscountPolicyByIdQuery(Guid PolicyId) : IQuery<DiscountPolicyReadModel>;
