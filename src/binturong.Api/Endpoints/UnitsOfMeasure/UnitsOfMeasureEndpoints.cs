using Application.Abstractions.Messaging;
using Application.Features.UnitsOfMeasure.Create;
using Application.Features.UnitsOfMeasure.Delete;
using Application.Features.UnitsOfMeasure.GetUnitOfMeasureById;
using Application.Features.UnitsOfMeasure.GetUnitsOfMeasure;
using Application.Features.UnitsOfMeasure.Update;
using Application.ReadModels.MasterData;

namespace Api.Endpoints.UnitsOfMeasure;

public sealed class UnitsOfMeasureEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/units-of-measure").WithTags("UnitsOfMeasure");

        // GET list
        // /api/units-of-measure?page=1&pageSize=50&search=kg
        group.MapGet(
            "/",
            async (
                int? page,
                int? pageSize,
                string? search,
                IQueryHandler<
                    GetUnitsOfMeasureQuery,
                    IReadOnlyList<UnitOfMeasureReadModel>
                > handler,
                CancellationToken ct
            ) =>
            {
                var query = new GetUnitsOfMeasureQuery(page ?? 1, pageSize ?? 50, search);
                var result = await handler.Handle(query, ct);

                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Ok(result.Value);
            }
        );

        // GET by id
        // /api/units-of-measure/{id}
        group.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                IQueryHandler<GetUnitOfMeasureByIdQuery, UnitOfMeasureReadModel> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new GetUnitOfMeasureByIdQuery(id), ct);
                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            }
        );

        // CREATE
        // POST /api/units-of-measure
        group.MapPost(
            "/",
            async (
                CreateUnitOfMeasureRequest req,
                ICommandHandler<CreateUnitOfMeasureCommand, Guid> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new CreateUnitOfMeasureCommand(req.Code, req.Name, req.IsActive);
                var result = await handler.Handle(cmd, ct);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created(
                    $"/api/units-of-measure/{result.Value}",
                    new { uomId = result.Value }
                );
            }
        );

        // UPDATE
        // PUT /api/units-of-measure/{id}
        group.MapPut(
            "/{id:guid}",
            async (
                Guid id,
                UpdateUnitOfMeasureRequest req,
                ICommandHandler<UpdateUnitOfMeasureCommand> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new UpdateUnitOfMeasureCommand(id, req.Code, req.Name, req.IsActive);
                var result = await handler.Handle(cmd, ct);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // DELETE
        // DELETE /api/units-of-measure/{id}
        group.MapDelete(
            "/{id:guid}",
            async (
                Guid id,
                ICommandHandler<DeleteUnitOfMeasureCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new DeleteUnitOfMeasureCommand(id), ct);
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );
    }
}
