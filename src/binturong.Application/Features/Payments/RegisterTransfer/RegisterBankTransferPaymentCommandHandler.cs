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

namespace Application.Features.Payments.RegisterTransfer;

internal sealed class RegisterBankTransferPaymentCommandHandler
    : ICommandHandler<RegisterBankTransferPaymentCommand, RegisterBankTransferPaymentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RegisterBankTransferPaymentCommandHandler(
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

    public async Task<Result<RegisterBankTransferPaymentResponse>> Handle(
        RegisterBankTransferPaymentCommand cmd,
        CancellationToken ct
    )
    {
        if (cmd.InvoiceId == Guid.Empty)
            return Result.Failure<RegisterBankTransferPaymentResponse>(
                Error.Validation("Payments.InvoiceRequired", "InvoiceId is required.")
            );

        if (cmd.ClientId == Guid.Empty)
            return Result.Failure<RegisterBankTransferPaymentResponse>(
                Domain.Payments.PaymentErrors.ClientRequired
            );

        if (cmd.PaymentMethodId == Guid.Empty)
            return Result.Failure<RegisterBankTransferPaymentResponse>(
                Domain.Payments.PaymentErrors.MethodRequired
            );

        if (cmd.Amount <= 0)
            return Result.Failure<RegisterBankTransferPaymentResponse>(
                Domain.Payments.PaymentErrors.AmountInvalid
            );

        if (string.IsNullOrWhiteSpace(cmd.Reference))
            return Result.Failure<RegisterBankTransferPaymentResponse>(
                Domain.Payments.PaymentErrors.ReferenceRequired
            );

        var invoice = await _db
            .Invoices.Include(x => x.PaymentDetails)
            .FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);

        if (invoice is null)
            return Result.Failure<RegisterBankTransferPaymentResponse>(
                InvoiceErrors.NotFound(cmd.InvoiceId)
            );

        if (invoice.TaxStatus != "Emitted")
            return Result.Failure<RegisterBankTransferPaymentResponse>(
                InvoiceErrors.NotEmitted(cmd.InvoiceId)
            );

        // HU-PAG-02 escenario 2: si comprobante no coincide con monto -> bloquear
        // (Aquí la regla mínima: si amount > pending => no coincide)
        var paid = invoice.PaymentDetails.Sum(x => x.AppliedAmount);
        var pending = Math.Max(0, invoice.Total - paid);

        if (cmd.Amount > pending)
            return Result.Failure<RegisterBankTransferPaymentResponse>(
                InvoiceErrors.PaymentExceedsPending
            );

        // Crear Payment
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            ClientId = cmd.ClientId,
            PaymentMethodId = cmd.PaymentMethodId,
            PaymentDate = cmd.PaymentDate,
            TotalAmount = cmd.Amount,
            Reference = cmd.Reference.Trim(),
            Notes = cmd.Notes ?? string.Empty,
        };
        payment.RaiseCreated();

        _db.Payments.Add(payment);

        // Si el banco aún NO confirma: NO aplicar a la factura; poner en verificación
        if (!cmd.IsBankConfirmed)
        {
            var r = invoice.SetPaymentInVerification("BankTransferPending", DateTime.UtcNow);
            if (r.IsFailure)
                return Result.Failure<RegisterBankTransferPaymentResponse>(r.Error);

            await _db.SaveChangesAsync(ct);

            await _bus.AuditAsync(
                _currentUser.UserId,
                "Payments",
                "Payment",
                payment.Id,
                "PAYMENT_TRANSFER_REGISTERED_PENDING",
                string.Empty,
                $"invoiceId={invoice.Id}; amount={cmd.Amount}; reference={cmd.Reference}",
                _request.IpAddress,
                _request.UserAgent,
                ct
            );

            return Result.Success(
                new RegisterBankTransferPaymentResponse(
                    payment.Id,
                    invoice.Id,
                    0m,
                    invoice.InternalStatus
                )
            );
        }

        // Confirmado: aplicar pago parcial o total según monto
        var detail = new PaymentDetail
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            InvoiceId = invoice.Id,
            AppliedAmount = cmd.Amount,
        };

        _db.PaymentDetails.Add(detail);
        invoice.PaymentDetails.Add(detail);
        invoice.ApplyPayment(cmd.Amount, DateTime.Now);

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payments",
            "Payment",
            payment.Id,
            "PAYMENT_TRANSFER_REGISTERED_CONFIRMED",
            string.Empty,
            $"invoiceId={invoice.Id}; amount={cmd.Amount}; reference={cmd.Reference}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(
            new RegisterBankTransferPaymentResponse(
                payment.Id,
                invoice.Id,
                cmd.Amount,
                invoice.InternalStatus
            )
        );
    }
}
