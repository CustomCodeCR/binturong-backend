using Application.Abstractions.Messaging;

namespace Application.Features.Branches.Delete;

public sealed record DeleteBranchCommand(Guid BranchId) : ICommand;
