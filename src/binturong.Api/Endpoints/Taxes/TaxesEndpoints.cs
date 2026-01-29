using Application.Abstractions.Messaging;
using Application.Features.Taxes.Create;
using Application.Features.Taxes.Delete;
using Application.Features.Taxes.GetTaxById;
using Application.Features.Taxes.GetTaxes;
using Application.Features.Taxes.Update;
using Application.ReadModels.MasterData;

namespace Api.Endpoints.Taxes;

public sealed class TaxesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/taxes").WithTags("Taxes");

        // =========================
        // GET list
        // /api/taxes?page=1&pageSize=50&search=vat
        // =========================
        group.MapGet(
            "/",
            async (
                int? page,
                int? pageSize,
                string? search,
                IQueryHandler<GetTaxesQuery, IReadOnlyList<TaxReadModel>> handler,
                CancellationToken ct
            ) =>
            {
                var query = new GetTaxesQuery(page ?? 1, pageSize ?? 50, search);
                var result = await handler.Handle(query, ct);

                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Ok(result.Value);
            }
        );

        // =========================
        // GET by id
        // /api/taxes/{id}
        // =========================
        group.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                IQueryHandler<GetTaxByIdQuery, TaxReadModel> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new GetTaxByIdQuery(id), ct);

                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            }
        );

        // =========================
        // CREATE
        // POST /api/taxes
        // =========================
        group.MapPost(
            "/",
            async (
                CreateTaxRequest req,
                ICommandHandler<CreateTaxCommand, Guid> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new CreateTaxCommand(req.Name, req.Code, req.Percentage, req.IsActive);

                var result = await handler.Handle(cmd, ct);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created($"/api/taxes/{result.Value}", new { taxId = result.Value });
            }
        );

        // =========================
        // UPDATE
        // PUT /api/taxes/{id}
        // =========================
        group.MapPut(
            "/{id:guid}",
            async (
                Guid id,
                UpdateTaxRequest req,
                ICommandHandler<UpdateTaxCommand> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new UpdateTaxCommand(
                    id,
                    req.Name,
                    req.Code,
                    req.Percentage,
                    req.IsActive
                );

                var result = await handler.Handle(cmd, ct);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // =========================
        // DELETE
        // DELETE /api/taxes/{id}
        // =========================
        group.MapDelete(
            "/{id:guid}",
            async (Guid id, ICommandHandler<DeleteTaxCommand> handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new DeleteTaxCommand(id), ct);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );
    }
}
