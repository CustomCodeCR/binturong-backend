using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Payments.GetPaymentById;
using Application.Features.Payments.GetPayments;
using Application.Features.Payments.RegisterCard;
using Application.Features.Payments.RegisterCash;
using Application.Features.Payments.RegisterPartial;
using Application.Features.Payments.RegisterTransfer;
using Application.Features.Payments.Reports.ExportPaymentHistoryExcel;
using Application.Features.Payments.Reports.ExportPaymentHistoryPdf;
using Application.ReadModels.Sales;
using Application.Security.Scopes;

namespace Api.Endpoints.Payments;

public sealed class PaymentsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments").WithTags("Payments");

        // =========================
        // HU-PAG-05: historial + filtros (Mongo)
        // GET /api/payments?page=&pageSize=&search=&paymentMethodId=
        // =========================
        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    Guid? paymentMethodId,
                    IQueryHandler<GetPaymentsQuery, IReadOnlyList<PaymentReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetPaymentsQuery(page ?? 1, pageSize ?? 50, search, paymentMethodId),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PaymentsRead);

        // =========================
        // CRUD: GetById (Mongo)
        // GET /api/payments/{id}
        // =========================
        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetPaymentByIdQuery, PaymentReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetPaymentByIdQuery(id), ct);

                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PaymentsRead);

        // =========================
        // HU-PAG-01: efectivo (cambio incluido)
        // POST /api/payments/register/cash
        // =========================
        group
            .MapPost(
                "/register/cash",
                async (
                    RegisterCashPaymentRequest req,
                    ICommandHandler<
                        RegisterCashPaymentCommand,
                        RegisterCashPaymentResponse
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RegisterCashPaymentCommand(
                            req.InvoiceId,
                            req.ClientId,
                            req.PaymentMethodId,
                            req.PaymentDate,
                            req.AmountTendered,
                            req.Notes
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PaymentsRegister);

        // =========================
        // HU-PAG-02: transferencia (confirmada / verificación)
        // POST /api/payments/register/transfer
        // =========================
        group
            .MapPost(
                "/register/transfer",
                async (
                    RegisterTransferPaymentRequest req,
                    ICommandHandler<
                        RegisterBankTransferPaymentCommand,
                        RegisterBankTransferPaymentResponse
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RegisterBankTransferPaymentCommand(
                            req.InvoiceId,
                            req.ClientId,
                            req.PaymentMethodId,
                            req.PaymentDate,
                            req.Amount,
                            req.Reference,
                            req.IsBankConfirmed,
                            req.Notes
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PaymentsRegister);

        // =========================
        // HU-PAG-03: tarjeta POS / datáfono
        // POST /api/payments/register/card
        // =========================
        group
            .MapPost(
                "/register/card",
                async (
                    RegisterCardPaymentRequest req,
                    ICommandHandler<
                        RegisterCardPaymentCommand,
                        RegisterCardPaymentResponse
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RegisterCardPaymentCommand(
                            req.InvoiceId,
                            req.ClientId,
                            req.PaymentMethodId,
                            req.PaymentDate,
                            req.Amount,
                            req.IsPosAvailable,
                            req.IsApproved,
                            req.PosAuthCode,
                            req.Notes
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PaymentsRegister);

        // =========================
        // HU-PAG-04: pago parcial
        // POST /api/payments/register/partial
        // =========================
        group
            .MapPost(
                "/register/partial",
                async (
                    RegisterPartialPaymentRequest req,
                    ICommandHandler<RegisterPartialPaymentCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RegisterPartialPaymentCommand(
                            req.InvoiceId,
                            req.ClientId,
                            req.PaymentMethodId,
                            req.PaymentDate,
                            req.Amount,
                            req.Reference,
                            req.Notes
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/payments/{result.Value}",
                            new { paymentId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.PaymentsRegister);

        // =========================
        // HU-PAG-05: export PDF
        // GET /api/payments/reports/history/pdf?from=&to=&clientId=&paymentMethodId=&search=
        // =========================
        group
            .MapGet(
                "/reports/history/pdf",
                async (
                    DateTime? from,
                    DateTime? to,
                    Guid? clientId,
                    Guid? paymentMethodId,
                    string? search,
                    ICommandHandler<ExportPaymentHistoryPdfCommand, byte[]> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportPaymentHistoryPdfCommand(
                            from,
                            to,
                            clientId,
                            paymentMethodId,
                            search
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(result.Value, "application/pdf", "payment_history.pdf");
                }
            )
            .RequireScope(SecurityScopes.PaymentsExport);

        // =========================
        // HU-PAG-05: export Excel
        // GET /api/payments/reports/history/excel?from=&to=&clientId=&paymentMethodId=&search=
        // =========================
        group
            .MapGet(
                "/reports/history/excel",
                async (
                    DateTime? from,
                    DateTime? to,
                    Guid? clientId,
                    Guid? paymentMethodId,
                    string? search,
                    ICommandHandler<ExportPaymentHistoryExcelCommand, byte[]> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportPaymentHistoryExcelCommand(
                            from,
                            to,
                            clientId,
                            paymentMethodId,
                            search
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "payment_history.xlsx"
                        );
                }
            )
            .RequireScope(SecurityScopes.PaymentsExport);
    }
}
