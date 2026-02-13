using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Payments.GetPaymentById;
using Application.Features.Payments.GetPayments;
using Application.Features.Payments.Register;
using Application.ReadModels.Sales;
using Application.Security.Scopes;

namespace Api.Endpoints.Payments;

public sealed class PaymentsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments").WithTags("Payments");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<GetPaymentsQuery, IReadOnlyList<PaymentReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetPaymentsQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PaymentsRead);

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

        group
            .MapPost(
                "/register",
                async (
                    RegisterPaymentRequest req,
                    ICommandHandler<RegisterPaymentCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    // FIRMA REAL:
                    // RegisterPaymentCommand(Guid ClientId, Guid PaymentMethodId, DateTime PaymentDate, decimal TotalAmount,
                    //   string Reference, string Notes, IReadOnlyList<ApplyInvoicePayment> AppliedInvoices)

                    var applied = (
                        req.AppliedInvoices ?? Array.Empty<RegisterPaymentAppliedInvoiceRequest>()
                    )
                        .Select(x => new ApplyInvoicePayment(x.InvoiceId, x.AppliedAmount))
                        .ToList();

                    var result = await handler.Handle(
                        new RegisterPaymentCommand(
                            req.ClientId,
                            req.PaymentMethodId,
                            req.PaymentDate,
                            req.TotalAmount,
                            req.Reference ?? string.Empty,
                            req.Notes ?? string.Empty,
                            applied
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
            // OJO: tu SecurityScopes NO tiene PaymentsRegister, usa PaymentsCreate (según tu código)
            .RequireScope(SecurityScopes.PaymentsCreate);
    }
}
