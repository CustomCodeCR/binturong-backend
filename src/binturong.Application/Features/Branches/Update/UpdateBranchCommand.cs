using Application.Abstractions.Messaging;

namespace Application.Features.Branches.Update;

public sealed record UpdateBranchCommand(
    Guid BranchId,
    string Code,
    string Name,
    string Address,
    string Phone,
    bool IsActive
) : ICommand;
