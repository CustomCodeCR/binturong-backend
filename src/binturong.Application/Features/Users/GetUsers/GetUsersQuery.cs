using Application.Abstractions.Messaging;
using Application.ReadModels.Security;

namespace Application.Features.Users.GetUsers;

public sealed record GetUsersQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<UserReadModel>>
{
    public int Take => PageSize;
    public int Skip => (Page - 1) * PageSize;
}
