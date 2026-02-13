using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Invoices.ConvertFromQuote;

internal sealed class ConvertQuoteToInvoiceCommandHandler
    : ICommandHandler<ConvertQuoteToInvoiceCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public ConvertQuoteToInvoiceCommandHandler(
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

    public async Task<Result<Guid>> Handle(ConvertQuoteToInvoiceCommand cmd, CancellationToken ct)
    {
        var quote = await _db
            .Quotes.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == cmd.QuoteId, ct);

        if (quote is null)
            return Result.Failure<Guid>(
                Error.NotFound("Quotes.NotFound", $"Quote '{cmd.QuoteId}' not found.")
            );

        if (!quote.AcceptedByClient || quote.Status != "Accepted")
            return Result.Failure<Guid>(InvoiceErrors.QuoteNotAccepted);

        // HU-COT-05 - Scenario 3: overdue receivables (example rule)
        var overdueCutoff = DateTime.UtcNow.AddDays(-30);
        var hasOverdue = await _db.Invoices.AnyAsync(
            x =>
                x.ClientId == quote.ClientId
                && x.InternalStatus != "Paid"
                && x.IssueDate < overdueCutoff,
            ct
        );

        if (hasOverdue)
            return Result.Failure<Guid>(InvoiceErrors.ClientCreditOverdue);

        // HU-COT-05 - Scenario 2: inventory insufficient
        // Plug your real inventory check here.
        // if (!await _inventory.HasStockAsync(quote, ct)) return Failure(InvoiceErrors.InventoryInsufficient);

        var invoice = new Domain.Invoices.Invoice
        {
            Id = Guid.NewGuid(),
            ClientId = quote.ClientId,
            BranchId = quote.BranchId,
            SalesOrderId = null,
            ContractId = null,
            IssueDate = cmd.IssueDate,
            DocumentType = cmd.DocumentType,
            Currency = quote.Currency,
            ExchangeRate = quote.ExchangeRate,
            Subtotal = quote.Subtotal,
            Taxes = quote.Taxes,
            Discounts = quote.Discounts,
            Total = quote.Total,
            TaxStatus = "Draft",
            InternalStatus = "Draft",
            EmailSent = false,
        };

        foreach (var d in quote.Details)
        {
            invoice.Details.Add(
                new Domain.InvoiceDetails.InvoiceDetail
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    ProductId = d.ProductId,
                    Description = string.Empty,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    DiscountPerc = d.DiscountPerc,
                    TaxPerc = d.TaxPerc,
                    LineTotal = d.LineTotal,
                }
            );
        }

        invoice.RaiseCreated();
        invoice.RaiseCreatedFromQuote(quote.Id, DateTime.Now);

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Invoices",
            "Invoice",
            invoice.Id,
            "INVOICE_CONVERTED_FROM_QUOTE",
            string.Empty,
            $"quoteId={quote.Id}; clientId={quote.ClientId}; total={invoice.Total}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(invoice.Id);
    }
}
