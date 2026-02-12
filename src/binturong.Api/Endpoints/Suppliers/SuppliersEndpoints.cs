using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Suppliers.Create;
using Application.Features.Suppliers.Delete;
using Application.Features.Suppliers.GetSupplierById;
using Application.Features.Suppliers.GetSuppliers;
using Application.Features.Suppliers.History.ExportSupplierPurchaseHistoryExcel;
using Application.Features.Suppliers.History.ExportSupplierPurchaseHistoryPdf;
using Application.Features.Suppliers.History.GetSupplierPurchaseHistory;
using Application.Features.Suppliers.SetCreditConditions;
using Application.Features.Suppliers.Update;
using Application.ReadModels.CRM;
using Application.ReadModels.Purchases;
using Application.Security.Scopes;

namespace Api.Endpoints.Suppliers;

public sealed class SuppliersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suppliers").WithTags("Suppliers");

        group
            .MapGet(
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
            )
            .RequireScope(SecurityScopes.SuppliersRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetSupplierByIdQuery, SupplierReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetSupplierByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.SuppliersRead);

        group
            .MapGet(
                "/{id:guid}/purchase-history",
                async (
                    Guid id,
                    DateTime? from,
                    DateTime? to,
                    string? status,
                    int? skip,
                    int? take,
                    IQueryHandler<
                        GetSupplierPurchaseHistoryQuery,
                        IReadOnlyList<PurchaseOrderReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetSupplierPurchaseHistoryQuery(
                            id,
                            from,
                            to,
                            status,
                            skip ?? 0,
                            take ?? 50
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.SuppliersRead);

        group
            .MapGet(
                "/{id:guid}/purchase-history/pdf",
                async (
                    Guid id,
                    DateTime? from,
                    DateTime? to,
                    string? status,
                    ICommandHandler<ExportSupplierPurchaseHistoryPdfCommand, byte[]> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportSupplierPurchaseHistoryPdfCommand(id, from, to, status),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value,
                            "application/pdf",
                            $"supplier_purchase_history_{id:N}.pdf"
                        );
                }
            )
            .RequireScope(SecurityScopes.SuppliersRead);

        group
            .MapGet(
                "/{id:guid}/purchase-history/excel",
                async (
                    Guid id,
                    DateTime? from,
                    DateTime? to,
                    string? status,
                    ICommandHandler<ExportSupplierPurchaseHistoryExcelCommand, byte[]> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportSupplierPurchaseHistoryExcelCommand(id, from, to, status),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"supplier_purchase_history_{id:N}.xlsx"
                        );
                }
            )
            .RequireScope(SecurityScopes.SuppliersRead);

        group
            .MapPost(
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
            )
            .RequireScope(SecurityScopes.SuppliersCreate);

        group
            .MapPut(
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

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.SuppliersUpdate);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ICommandHandler<DeleteSupplierCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new DeleteSupplierCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.SuppliersDelete);

        group
            .MapPut(
                "/{id:guid}/credit",
                async (
                    Guid id,
                    SetSupplierCreditRequest req,
                    ICommandHandler<SetSupplierCreditConditionsCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var hasPermission = true;

                    var cmd = new SetSupplierCreditConditionsCommand(
                        id,
                        req.CreditLimit,
                        req.CreditDays,
                        hasPermission
                    );

                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.SuppliersCreditAssign);
    }
}
