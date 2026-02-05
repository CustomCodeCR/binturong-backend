using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.Branches.GetBranchById;

public sealed record GetBranchByIdQuery(Guid BranchId) : IQuery<BranchReadModel>;
