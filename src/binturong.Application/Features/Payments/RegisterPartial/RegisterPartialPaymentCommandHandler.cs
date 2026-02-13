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

namespace Application.Features.Payments.RegisterPartial;

internal sealed class RegisterPartialPaymentCommandHandler
    : ICommandHandler<RegisterPartialPaymentCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RegisterPartialPaymentCommandHandler(
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

    public async Task<Result<Guid>> Handle(RegisterPartialPaymentCommand cmd, CancellationToken ct)
    {
        if (cmd.InvoiceId == Guid.Empty)
            return Result.Failure<Guid>(
                Error.Validation("Payments.InvoiceRequired", "InvoiceId is required.")
            );

        if (cmd.ClientId == Guid.Empty)
            return Result.Failure<Guid>(Domain.Payments.PaymentErrors.ClientRequired);

        if (cmd.PaymentMethodId == Guid.Empty)
            return Result.Failure<Guid>(Domain.Payments.PaymentErrors.MethodRequired);

        if (cmd.Amount <= 0)
            return Result.Failure<Guid>(Domain.Payments.PaymentErrors.AmountInvalid);

        var invoice = await _db
            .Invoices.Include(x => x.PaymentDetails)
            .FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);

        if (invoice is null)
            return Result.Failure<Guid>(InvoiceErrors.NotFound(cmd.InvoiceId));

        if (invoice.TaxStatus != "Emitted")
            return Result.Failure<Guid>(InvoiceErrors.NotEmitted(cmd.InvoiceId));

        // HU-PAG-04 escenario 2: bloquear si excede saldo
        var v = invoice.ValidateApplyPaymentAmount(cmd.Amount);
        if (v.IsFailure)
            return Result.Failure<Guid>(v.Error);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            ClientId = cmd.ClientId,
            PaymentMethodId = cmd.PaymentMethodId,
            PaymentDate = cmd.PaymentDate,
            TotalAmount = cmd.Amount,
            Reference = cmd.Reference ?? string.Empty,
            Notes = cmd.Notes ?? string.Empty,
        };
        payment.RaiseCreated();

        var detail = new PaymentDetail
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            InvoiceId = invoice.Id,
            AppliedAmount = cmd.Amount,
        };

        _db.Payments.Add(payment);
        _db.PaymentDetails.Add(detail);

        invoice.PaymentDetails.Add(detail);
        invoice.ApplyPayment(cmd.Amount, DateTime.Now);

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payments",
            "Payment",
            payment.Id,
            "PAYMENT_PARTIAL_REGISTERED",
            string.Empty,
            $"invoiceId={invoice.Id}; amount={cmd.Amount}; status={invoice.InternalStatus}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(payment.Id);
    }
}
