using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Invoices.Update;

internal sealed class UpdateInvoiceCommandHandler : ICommandHandler<UpdateInvoiceCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateInvoiceCommandHandler(
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

    public async Task<Result> Handle(UpdateInvoiceCommand cmd, CancellationToken ct)
    {
        var invoice = await _db.Invoices.FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);
        if (invoice is null)
            return Result.Failure(InvoiceErrors.NotFound(cmd.InvoiceId));

        invoice.ClientId = cmd.ClientId;
        invoice.BranchId = cmd.BranchId;
        invoice.IssueDate = cmd.IssueDate;
        invoice.Currency = cmd.Currency;
        invoice.ExchangeRate = cmd.ExchangeRate;
        invoice.Subtotal = cmd.Subtotal;
        invoice.Taxes = cmd.Taxes;
        invoice.Discounts = cmd.Discounts;
        invoice.Total = cmd.Total;

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
