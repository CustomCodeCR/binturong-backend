using Application.Abstractions.Messaging;

namespace Application.Features.Branches.Create;

public sealed record CreateBranchCommand(
    string Code,
    string Name,
    string Address,
    string Phone,
    bool IsActive = true
) : ICommand<Guid>;
