using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.PaymentDetails;
using Domain.Payments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payments.Register;

internal sealed class RegisterPaymentCommandHandler : ICommandHandler<RegisterPaymentCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RegisterPaymentCommandHandler(
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

    public async Task<Result<Guid>> Handle(RegisterPaymentCommand cmd, CancellationToken ct)
    {
        if (cmd.ClientId == Guid.Empty)
            return Result.Failure<Guid>(
                Error.Validation("Payments.ClientRequired", "ClientId is required.")
            );

        if (cmd.PaymentMethodId == Guid.Empty)
            return Result.Failure<Guid>(
                Error.Validation("Payments.MethodRequired", "PaymentMethodId is required.")
            );

        if (cmd.TotalAmount <= 0)
            return Result.Failure<Guid>(
                Error.Validation("Payments.AmountInvalid", "TotalAmount must be > 0.")
            );

        if (cmd.AppliedInvoices.Count == 0)
            return Result.Failure<Guid>(
                Error.Validation(
                    "Payments.AppliedRequired",
                    "At least one invoice must be applied."
                )
            );

        var sumApplied = cmd.AppliedInvoices.Sum(x => x.AppliedAmount);
        if (sumApplied <= 0 || sumApplied > cmd.TotalAmount)
            return Result.Failure<Guid>(
                Error.Validation(
                    "Payments.AppliedInvalid",
                    "Applied sum must be > 0 and <= TotalAmount."
                )
            );

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            ClientId = cmd.ClientId,
            PaymentMethodId = cmd.PaymentMethodId,
            PaymentDate = cmd.PaymentDate,
            TotalAmount = cmd.TotalAmount,
            Reference = cmd.Reference ?? string.Empty,
            Notes = cmd.Notes ?? string.Empty,
        };

        foreach (var a in cmd.AppliedInvoices)
        {
            if (a.InvoiceId == Guid.Empty || a.AppliedAmount <= 0)
                return Result.Failure<Guid>(
                    Error.Validation("Payments.AppliedLineInvalid", "Invalid applied invoice line.")
                );

            var inv = await _db
                .Invoices.Include(x => x.PaymentDetails)
                .FirstOrDefaultAsync(x => x.Id == a.InvoiceId, ct);

            if (inv is null)
                return Result.Failure<Guid>(
                    Error.NotFound("Invoices.NotFound", $"Invoice '{a.InvoiceId}' not found.")
                );

            // HU-FAC-03: pending / partial / paid
            var paidSoFar = inv.PaymentDetails.Sum(d => d.AppliedAmount);
            var newPaid = paidSoFar + a.AppliedAmount;

            inv.InternalStatus = newPaid >= inv.Total ? "Paid" : "Pending";

            payment.Details.Add(
                new PaymentDetail
                {
                    Id = Guid.NewGuid(),
                    PaymentId = payment.Id,
                    InvoiceId = inv.Id,
                    AppliedAmount = a.AppliedAmount,
                }
            );
        }

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payments",
            "Payment",
            payment.Id,
            "PAYMENT_REGISTERED",
            string.Empty,
            $"clientId={payment.ClientId}; total={payment.TotalAmount}; applied={sumApplied}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(payment.Id);
    }
}
