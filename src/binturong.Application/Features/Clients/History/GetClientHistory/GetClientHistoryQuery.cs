using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.Clients.History.GetClientHistory;

public sealed record GetClientHistoryQuery(
    Guid ClientId,
    DateTime? From,
    DateTime? To,
    string? Status,
    int Skip = 0,
    int Take = 50
) : IQuery<IReadOnlyList<SalesOrderReadModel>>;
