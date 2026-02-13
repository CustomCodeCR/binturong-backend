using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Invoices;
using Domain.PaymentDetails;
using Domain.Payments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payments.RegisterCash;

internal sealed class RegisterCashPaymentCommandHandler
    : ICommandHandler<RegisterCashPaymentCommand, RegisterCashPaymentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RegisterCashPaymentCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<RegisterCashPaymentResponse>> Handle(
        RegisterCashPaymentCommand cmd,
        CancellationToken ct
    )
    {
        if (cmd.InvoiceId == Guid.Empty)
            return Result.Failure<RegisterCashPaymentResponse>(
                Error.Validation("Payments.InvoiceRequired", "InvoiceId is required.")
            );

        if (cmd.ClientId == Guid.Empty)
            return Result.Failure<RegisterCashPaymentResponse>(
                Domain.Payments.PaymentErrors.ClientRequired
            );

        if (cmd.PaymentMethodId == Guid.Empty)
            return Result.Failure<RegisterCashPaymentResponse>(
                Domain.Payments.PaymentErrors.MethodRequired
            );

        if (cmd.AmountTendered <= 0)
            return Result.Failure<RegisterCashPaymentResponse>(
                Domain.Payments.PaymentErrors.AmountInvalid
            );

        var invoice = await _db
            .Invoices.Include(x => x.PaymentDetails)
            .FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);

        if (invoice is null)
            return Result.Failure<RegisterCashPaymentResponse>(
                InvoiceErrors.NotFound(cmd.InvoiceId)
            );

        if (invoice.TaxStatus != "Emitted")
            return Result.Failure<RegisterCashPaymentResponse>(
                InvoiceErrors.NotEmitted(cmd.InvoiceId)
            );

        // HU-PAG-01: si monto < total pendiente -> rechazar
        var paid = invoice.PaymentDetails.Sum(x => x.AppliedAmount);
        var pending = Math.Max(0, invoice.Total - paid);

        if (cmd.AmountTendered < pending)
            return Result.Failure<RegisterCashPaymentResponse>(
                InvoiceErrors.PaymentInsufficientToSettle
            );

        var applyAmount = pending; // se aplica justo lo que faltaba
        var change = cmd.AmountTendered - pending;

        // Crear Payment
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            ClientId = cmd.ClientId,
            PaymentMethodId = cmd.PaymentMethodId,
            PaymentDate = cmd.PaymentDate,
            TotalAmount = cmd.AmountTendered,
            Reference = "CASH",
            Notes = cmd.Notes ?? string.Empty,
        };
        payment.RaiseCreated();

        // Crear PaymentDetail
        var detail = new PaymentDetail
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            InvoiceId = invoice.Id,
            AppliedAmount = applyAmount,
        };

        _db.Payments.Add(payment);
        _db.PaymentDetails.Add(detail);

        // actualizar invoice + evento
        invoice.PaymentDetails.Add(detail);
        invoice.ApplyPayment(applyAmount, DateTime.Now);

        await _db.SaveChangesAsync(ct);

        // Audit
        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payments",
            "Payment",
            payment.Id,
            "PAYMENT_CASH_REGISTERED",
            string.Empty,
            $"invoiceId={invoice.Id}; tendered={cmd.AmountTendered}; applied={applyAmount}; change={change}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(
            new RegisterCashPaymentResponse(
                payment.Id,
                invoice.Id,
                invoice.Total,
                applyAmount,
                change,
                invoice.InternalStatus
            )
        );
    }
}
