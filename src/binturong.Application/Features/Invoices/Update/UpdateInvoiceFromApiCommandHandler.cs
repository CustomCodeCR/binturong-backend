using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Invoices.Update;

internal sealed class UpdateInvoiceFromApiCommandHandler
    : ICommandHandler<UpdateInvoiceFromApiCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateInvoiceFromApiCommandHandler(
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

    public async Task<Result> Handle(UpdateInvoiceFromApiCommand cmd, CancellationToken ct)
    {
        var invoice = await _db
            .Invoices.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);

        if (invoice is null)
            return Result.Failure(InvoiceErrors.NotFound(cmd.InvoiceId));

        // Campos que realmente vienen del API
        invoice.IssueDate = cmd.IssueDate;
        invoice.DocumentType = cmd.DocumentType;
        invoice.Currency = cmd.Currency;
        invoice.ExchangeRate = cmd.ExchangeRate;
        invoice.InternalStatus = cmd.InternalStatus;

        // Notes no existe en Invoice entity que pegaste, entonces:
        // - si ya lo agregaste en tu entity, úsalo aquí.
        // - si NO existe, quita esta línea o agrega el campo en Invoice.
        // invoice.Notes = cmd.Notes ?? string.Empty;

        // Recalcular totales desde Details (fuente de verdad)
        var subtotal = invoice.Details.Sum(x => x.Quantity * x.UnitPrice);

        var discounts = invoice.Details.Sum(x =>
        {
            var baseAmount = x.Quantity * x.UnitPrice;
            return baseAmount * (x.DiscountPerc / 100m);
        });

        var taxes = invoice.Details.Sum(x =>
        {
            var baseAmount = x.Quantity * x.UnitPrice;
            var discountAmount = baseAmount * (x.DiscountPerc / 100m);
            var taxable = baseAmount - discountAmount;
            return taxable * (x.TaxPerc / 100m);
        });

        invoice.Subtotal = Math.Round(subtotal, 2);
        invoice.Discounts = Math.Round(discounts, 2);
        invoice.Taxes = Math.Round(taxes, 2);
        invoice.Total = Math.Round((invoice.Subtotal - invoice.Discounts) + invoice.Taxes, 2);

        invoice.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Invoices",
            "Invoice",
            invoice.Id,
            "INVOICE_UPDATED",
            string.Empty,
            $"total={invoice.Total}; taxStatus={invoice.TaxStatus}; internalStatus={invoice.InternalStatus}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
