using Application.Abstractions.Messaging;
using Application.ReadModels.CRM;

namespace Application.Features.Clients.GetClients;

public sealed record GetClientsQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<ClientReadModel>>
{
    public int Take => PageSize;
    public int Skip => (Page - 1) * PageSize;
}
