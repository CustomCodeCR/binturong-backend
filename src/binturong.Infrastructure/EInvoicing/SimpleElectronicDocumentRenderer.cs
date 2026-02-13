using System.Text;
using System.Xml.Linq;
using Application.Abstractions.Data;
using Application.Abstractions.EInvoicing;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EInvoicing;

public sealed class SimpleElectronicDocumentRenderer : IElectronicDocumentRenderer
{
    private readonly IApplicationDbContext _db;

    public SimpleElectronicDocumentRenderer(IApplicationDbContext db) => _db = db;

    public async Task<RenderedElectronicDocument> RenderInvoiceAsync(
        Guid invoiceId,
        CancellationToken ct
    )
    {
        var inv = await _db
            .Invoices.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == invoiceId, ct);

        if (inv is null)
            throw new InvalidOperationException($"Invoice '{invoiceId}' not found.");

        var xml = BuildInvoiceXml(inv);
        var xmlBytes = Encoding.UTF8.GetBytes(xml.ToString(SaveOptions.DisableFormatting));

        // PDF stub (placeholder bytes). Replace later with real renderer.
        var pdfBytes = Encoding.UTF8.GetBytes(
            $"PDF-INVOICE {inv.Id} {inv.Consecutive} {inv.Total}"
        );

        return new RenderedElectronicDocument(
            XmlFileName: $"invoice_{inv.Id}.xml",
            PdfFileName: $"invoice_{inv.Id}.pdf",
            XmlContentType: "application/xml",
            PdfContentType: "application/pdf",
            XmlBytes: xmlBytes,
            PdfBytes: pdfBytes
        );
    }

    public async Task<RenderedElectronicDocument> RenderCreditNoteAsync(
        Guid creditNoteId,
        CancellationToken ct
    )
    {
        var cn = await _db.CreditNotes.FirstOrDefaultAsync(x => x.Id == creditNoteId, ct);
        if (cn is null)
            throw new InvalidOperationException($"CreditNote '{creditNoteId}' not found.");

        var xml = new XDocument(
            new XElement(
                "CreditNote",
                new XElement("Id", cn.Id),
                new XElement("InvoiceId", cn.InvoiceId),
                new XElement("IssueDate", cn.IssueDate.ToString("O")),
                new XElement("Reason", cn.Reason),
                new XElement("TotalAmount", cn.TotalAmount)
            )
        );

        var xmlBytes = Encoding.UTF8.GetBytes(xml.ToString(SaveOptions.DisableFormatting));
        var pdfBytes = Encoding.UTF8.GetBytes(
            $"PDF-CREDITNOTE {cn.Id} {cn.Consecutive} {cn.TotalAmount}"
        );

        return new RenderedElectronicDocument(
            $"credit_note_{cn.Id}.xml",
            $"credit_note_{cn.Id}.pdf",
            "application/xml",
            "application/pdf",
            xmlBytes,
            pdfBytes
        );
    }

    public async Task<RenderedElectronicDocument> RenderDebitNoteAsync(
        Guid debitNoteId,
        CancellationToken ct
    )
    {
        var dn = await _db.DebitNotes.FirstOrDefaultAsync(x => x.Id == debitNoteId, ct);
        if (dn is null)
            throw new InvalidOperationException($"DebitNote '{debitNoteId}' not found.");

        var xml = new XDocument(
            new XElement(
                "DebitNote",
                new XElement("Id", dn.Id),
                new XElement("InvoiceId", dn.InvoiceId),
                new XElement("IssueDate", dn.IssueDate.ToString("O")),
                new XElement("Reason", dn.Reason),
                new XElement("TotalAmount", dn.TotalAmount)
            )
        );

        var xmlBytes = Encoding.UTF8.GetBytes(xml.ToString(SaveOptions.DisableFormatting));
        var pdfBytes = Encoding.UTF8.GetBytes(
            $"PDF-DEBITNOTE {dn.Id} {dn.Consecutive} {dn.TotalAmount}"
        );

        return new RenderedElectronicDocument(
            $"debit_note_{dn.Id}.xml",
            $"debit_note_{dn.Id}.pdf",
            "application/xml",
            "application/pdf",
            xmlBytes,
            pdfBytes
        );
    }

    private static XDocument BuildInvoiceXml(Domain.Invoices.Invoice inv)
    {
        return new XDocument(
            new XElement(
                "Invoice",
                new XElement("Id", inv.Id),
                new XElement("ClientId", inv.ClientId),
                new XElement("BranchId", inv.BranchId?.ToString() ?? ""),
                new XElement("IssueDate", inv.IssueDate.ToString("O")),
                new XElement("Currency", inv.Currency),
                new XElement("Subtotal", inv.Subtotal),
                new XElement("Taxes", inv.Taxes),
                new XElement("Discounts", inv.Discounts),
                new XElement("Total", inv.Total),
                new XElement(
                    "Lines",
                    inv.Details.Select(d => new XElement(
                        "Line",
                        new XElement("ProductId", d.ProductId),
                        new XElement("Description", d.Description),
                        new XElement("Quantity", d.Quantity),
                        new XElement("UnitPrice", d.UnitPrice),
                        new XElement("DiscountPerc", d.DiscountPerc),
                        new XElement("TaxPerc", d.TaxPerc),
                        new XElement("LineTotal", d.LineTotal)
                    ))
                )
            )
        );
    }
}
