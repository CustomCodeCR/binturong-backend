using Application.Abstractions.Messaging;

namespace Application.Features.InventoryTransfers.RequestReview;

public sealed record RequestInventoryTransferReviewCommand(Guid TransferId) : ICommand;
