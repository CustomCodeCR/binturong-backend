using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Invoices.Create;

internal sealed class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateInvoiceCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateInvoiceCommand cmd, CancellationToken ct)
    {
        if (cmd.ClientId == Guid.Empty)
            return Result.Failure<Guid>(InvoiceErrors.ClientRequired);

        if (cmd.Lines.Count == 0)
            return Result.Failure<Guid>(InvoiceErrors.NoLines);

        var clientExists = await _db.Clients.AnyAsync(x => x.Id == cmd.ClientId, ct);
        if (!clientExists)
            return Result.Failure<Guid>(
                Error.NotFound("Clients.NotFound", $"Client '{cmd.ClientId}' not found.")
            );

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ClientId = cmd.ClientId,
            BranchId = cmd.BranchId,
            SalesOrderId = cmd.SalesOrderId,
            ContractId = cmd.ContractId,
            IssueDate = cmd.IssueDate,
            DocumentType = cmd.DocumentType,
            Currency = cmd.Currency,
            ExchangeRate = cmd.ExchangeRate,
            Subtotal = cmd.Subtotal,
            Taxes = cmd.Taxes,
            Discounts = cmd.Discounts,
            Total = cmd.Total,
            TaxStatus = "Draft",
            InternalStatus = "Draft",
            EmailSent = false,
        };

        foreach (var l in cmd.Lines)
        {
            invoice.Details.Add(
                new Domain.InvoiceDetails.InvoiceDetail
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    ProductId = l.ProductId,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    DiscountPerc = l.DiscountPerc,
                    TaxPerc = l.TaxPerc,
                    LineTotal = l.LineTotal,
                }
            );
        }

        invoice.RaiseCreated();

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Invoices",
            "Invoice",
            invoice.Id,
            "INVOICE_CREATED",
            string.Empty,
            $"clientId={invoice.ClientId}; total={invoice.Total}; currency={invoice.Currency}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(invoice.Id);
    }
}
