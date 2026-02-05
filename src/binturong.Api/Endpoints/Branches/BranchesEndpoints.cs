using Application.Abstractions.Messaging;
using Application.Features.Branches.Create;
using Application.Features.Branches.Delete;
using Application.Features.Branches.GetBranchById;
using Application.Features.Branches.GetBranches;
using Application.Features.Branches.Update;
using Application.ReadModels.MasterData;

namespace Api.Endpoints.Branches;

public sealed class BranchesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/branches").WithTags("Branches");

        group.MapGet(
            "/",
            async (
                int? page,
                int? pageSize,
                string? search,
                IQueryHandler<GetBranchesQuery, IReadOnlyList<BranchReadModel>> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new GetBranchesQuery(page ?? 1, pageSize ?? 50, search),
                    ct
                );
                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Ok(result.Value);
            }
        );

        group.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                IQueryHandler<GetBranchByIdQuery, BranchReadModel> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new GetBranchByIdQuery(id), ct);
                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            }
        );

        group.MapPost(
            "/",
            async (
                CreateBranchRequest req,
                ICommandHandler<CreateBranchCommand, Guid> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new CreateBranchCommand(
                        req.Code,
                        req.Name,
                        req.Address,
                        req.Phone,
                        req.IsActive
                    ),
                    ct
                );
                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Created(
                        $"/api/branches/{result.Value}",
                        new { branchId = result.Value }
                    );
            }
        );

        group.MapPut(
            "/{id:guid}",
            async (
                Guid id,
                UpdateBranchRequest req,
                ICommandHandler<UpdateBranchCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new UpdateBranchCommand(
                        id,
                        req.Code,
                        req.Name,
                        req.Address,
                        req.Phone,
                        req.IsActive
                    ),
                    ct
                );
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        group.MapDelete(
            "/{id:guid}",
            async (Guid id, ICommandHandler<DeleteBranchCommand> handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new DeleteBranchCommand(id), ct);
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );
    }
}
