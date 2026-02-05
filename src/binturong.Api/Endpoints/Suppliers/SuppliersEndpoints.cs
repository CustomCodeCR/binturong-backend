using Application.Abstractions.Messaging;
using Application.Features.Suppliers.Create;
using Application.Features.Suppliers.Delete;
using Application.Features.Suppliers.GetSupplierById;
using Application.Features.Suppliers.GetSuppliers;
using Application.Features.Suppliers.SetCreditConditions;
using Application.Features.Suppliers.Update;
using Application.ReadModels.CRM;

namespace Api.Endpoints.Suppliers;

public sealed class SuppliersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suppliers").WithTags("Suppliers");

        // =========================
        // GET list
        // /api/suppliers?page=1&pageSize=50&search=acme
        // =========================
        group.MapGet(
            "/",
            async (
                int? page,
                int? pageSize,
                string? search,
                IQueryHandler<GetSuppliersQuery, IReadOnlyList<SupplierReadModel>> handler,
                CancellationToken ct
            ) =>
            {
                var query = new GetSuppliersQuery(page ?? 1, pageSize ?? 50, search);
                var result = await handler.Handle(query, ct);

                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Ok(result.Value);
            }
        );

        // =========================
        // GET by id
        // /api/suppliers/{id}
        // =========================
        group.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                IQueryHandler<GetSupplierByIdQuery, SupplierReadModel> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new GetSupplierByIdQuery(id), ct);
                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            }
        );

        // =========================
        // CREATE
        // POST /api/suppliers
        // =========================
        group.MapPost(
            "/",
            async (
                CreateSupplierRequest req,
                ICommandHandler<CreateSupplierCommand, Guid> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new CreateSupplierCommand(
                    req.IdentificationType,
                    req.Identification,
                    req.LegalName,
                    req.TradeName,
                    req.Email,
                    req.Phone,
                    req.PaymentTerms,
                    req.MainCurrency,
                    req.IsActive
                );

                var result = await handler.Handle(cmd, ct);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created(
                    $"/api/suppliers/{result.Value}",
                    new { supplierId = result.Value }
                );
            }
        );

        // =========================
        // UPDATE
        // PUT /api/suppliers/{id}
        // =========================
        group.MapPut(
            "/{id:guid}",
            async (
                Guid id,
                UpdateSupplierRequest req,
                ICommandHandler<UpdateSupplierCommand> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new UpdateSupplierCommand(
                    id,
                    req.LegalName,
                    req.TradeName,
                    req.Email,
                    req.Phone,
                    req.PaymentTerms,
                    req.MainCurrency,
                    req.IsActive
                );

                var result = await handler.Handle(cmd, ct);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // =========================
        // DELETE
        // DELETE /api/suppliers/{id}
        // =========================
        group.MapDelete(
            "/{id:guid}",
            async (Guid id, ICommandHandler<DeleteSupplierCommand> handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new DeleteSupplierCommand(id), ct);
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        group.MapPut(
            "/{id:guid}/credit",
            async (
                Guid id,
                SetSupplierCreditRequest req,
                ICommandHandler<SetSupplierCreditConditionsCommand> handler,
                CancellationToken ct
            ) =>
            {
                // 🔐 Replace with real authorization later
                var hasPermission = true;

                var cmd = new SetSupplierCreditConditionsCommand(
                    id,
                    req.CreditLimit,
                    req.CreditDays,
                    hasPermission
                );

                var result = await handler.Handle(cmd, ct);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );
    }
}
