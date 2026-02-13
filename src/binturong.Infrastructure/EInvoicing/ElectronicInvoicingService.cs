using Application.Abstractions.Data;
using Application.Abstractions.EInvoicing;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EInvoicing;

public sealed class ElectronicInvoicingService : IElectronicInvoicingService
{
    private readonly IApplicationDbContext _db;
    private readonly IElectronicDocumentRenderer _renderer;
    private readonly IDocumentStorage _storage;
    private readonly IHaciendaClient _hacienda;
    private readonly IExternalServiceHealth _health;
    private readonly IConsecutiveGenerator _consecutive;
    private readonly ITaxKeyGenerator _taxKey;

    public ElectronicInvoicingService(
        IApplicationDbContext db,
        IElectronicDocumentRenderer renderer,
        IDocumentStorage storage,
        IHaciendaClient hacienda,
        IExternalServiceHealth health,
        IConsecutiveGenerator consecutive,
        ITaxKeyGenerator taxKey
    )
    {
        _db = db;
        _renderer = renderer;
        _storage = storage;
        _hacienda = hacienda;
        _health = health;
        _consecutive = consecutive;
        _taxKey = taxKey;
    }

    public async Task<EInvoicingResult> EmitInvoiceAsync(Guid invoiceId, CancellationToken ct)
    {
        var inv =
            await _db.Invoices.FirstOrDefaultAsync(x => x.Id == invoiceId, ct)
            ?? throw new InvalidOperationException($"Invoice '{invoiceId}' not found.");

        var taxKey = string.IsNullOrWhiteSpace(inv.TaxKey)
            ? await _taxKey.GenerateAsync("INV", ct)
            : inv.TaxKey;

        var consecutive = string.IsNullOrWhiteSpace(inv.Consecutive)
            ? await _consecutive.NextAsync("INV", ct)
            : inv.Consecutive;

        var doc = await _renderer.RenderInvoiceAsync(invoiceId, ct);

        var xmlKey = $"invoices/{invoiceId}/{doc.XmlFileName}";
        var pdfKey = $"invoices/{invoiceId}/{doc.PdfFileName}";

        await _storage.PutAsync(xmlKey, doc.XmlContentType, doc.XmlBytes, ct);
        await _storage.PutAsync(pdfKey, doc.PdfContentType, doc.PdfBytes, ct);

        var haciendaUp = await _health.IsHaciendaUpAsync(ct);

        if (!haciendaUp)
        {
            inv.TaxKey = taxKey;
            inv.Consecutive = consecutive;
            inv.XmlS3Key = xmlKey;
            inv.PdfS3Key = pdfKey;
            inv.TaxStatus = "Contingency";
            inv.InternalStatus = "PendingResend";
            inv.RaiseContingencyActivated(DateTime.UtcNow);

            await _db.SaveChangesAsync(ct);

            return new EInvoicingResult(
                true,
                "Contingency",
                inv.TaxStatus,
                taxKey,
                consecutive,
                pdfKey,
                xmlKey,
                "Hacienda down: stored as contingency."
            );
        }

        var submit = await _hacienda.SubmitAsync(
            new HaciendaSubmitRequest(
                "INV",
                taxKey,
                consecutive,
                Convert.ToBase64String(doc.XmlBytes)
            ),
            ct
        );

        inv.TaxKey = taxKey;
        inv.Consecutive = consecutive;
        inv.XmlS3Key = xmlKey;
        inv.PdfS3Key = pdfKey;

        if (!submit.IsSuccess || submit.Status == "Rejected")
        {
            inv.TaxStatus = "Rejected";
            inv.InternalStatus = "Error";
            inv.RaiseEmissionRejected(submit.Message ?? "Rejected by Hacienda", DateTime.UtcNow);
            await _db.SaveChangesAsync(ct);

            return new EInvoicingResult(
                false,
                "Normal",
                inv.TaxStatus,
                taxKey,
                consecutive,
                pdfKey,
                xmlKey,
                submit.Message
            );
        }

        inv.TaxStatus = submit.Status == "Accepted" ? "Emitted" : "Processing";
        inv.InternalStatus = "Ok";
        inv.EmailSent = false;

        inv.RaiseEmitted(DateTime.UtcNow);
        await _db.SaveChangesAsync(ct);

        return new EInvoicingResult(
            true,
            "Normal",
            inv.TaxStatus,
            taxKey,
            consecutive,
            pdfKey,
            xmlKey,
            submit.Message
        );
    }

    public async Task<EInvoicingResult> EmitCreditNoteAsync(Guid creditNoteId, CancellationToken ct)
    {
        var cn =
            await _db.CreditNotes.FirstOrDefaultAsync(x => x.Id == creditNoteId, ct)
            ?? throw new InvalidOperationException($"CreditNote '{creditNoteId}' not found.");

        var taxKey = string.IsNullOrWhiteSpace(cn.TaxKey)
            ? await _taxKey.GenerateAsync("NC", ct)
            : cn.TaxKey;

        var consecutive = string.IsNullOrWhiteSpace(cn.Consecutive)
            ? await _consecutive.NextAsync("NC", ct)
            : cn.Consecutive;

        var doc = await _renderer.RenderCreditNoteAsync(creditNoteId, ct);

        var xmlKey = $"credit_notes/{creditNoteId}/{doc.XmlFileName}";
        var pdfKey = $"credit_notes/{creditNoteId}/{doc.PdfFileName}";

        await _storage.PutAsync(xmlKey, doc.XmlContentType, doc.XmlBytes, ct);
        await _storage.PutAsync(pdfKey, doc.PdfContentType, doc.PdfBytes, ct);

        var haciendaUp = await _health.IsHaciendaUpAsync(ct);

        if (!haciendaUp)
        {
            cn.TaxKey = taxKey;
            cn.Consecutive = consecutive;
            cn.XmlS3Key = xmlKey;
            cn.PdfS3Key = pdfKey;
            cn.ActivateContingency(DateTime.UtcNow);

            await _db.SaveChangesAsync(ct);

            return new EInvoicingResult(
                true,
                "Contingency",
                cn.TaxStatus,
                taxKey,
                consecutive,
                pdfKey,
                xmlKey,
                "Hacienda down."
            );
        }

        var submit = await _hacienda.SubmitAsync(
            new HaciendaSubmitRequest(
                "NC",
                taxKey,
                consecutive,
                Convert.ToBase64String(doc.XmlBytes)
            ),
            ct
        );

        if (!submit.IsSuccess || submit.Status == "Rejected")
        {
            cn.TaxStatus = "Rejected";
            cn.RaiseUpdated();
            await _db.SaveChangesAsync(ct);

            return new EInvoicingResult(
                false,
                "Normal",
                cn.TaxStatus,
                taxKey,
                consecutive,
                pdfKey,
                xmlKey,
                submit.Message
            );
        }

        cn.MarkEmitted(taxKey, consecutive, pdfKey, xmlKey, DateTime.UtcNow);
        await _db.SaveChangesAsync(ct);

        return new EInvoicingResult(
            true,
            "Normal",
            cn.TaxStatus,
            taxKey,
            consecutive,
            pdfKey,
            xmlKey,
            submit.Message
        );
    }

    public async Task<EInvoicingResult> EmitDebitNoteAsync(Guid debitNoteId, CancellationToken ct)
    {
        var dn =
            await _db.DebitNotes.FirstOrDefaultAsync(x => x.Id == debitNoteId, ct)
            ?? throw new InvalidOperationException($"DebitNote '{debitNoteId}' not found.");

        var taxKey = string.IsNullOrWhiteSpace(dn.TaxKey)
            ? await _taxKey.GenerateAsync("ND", ct)
            : dn.TaxKey;

        var consecutive = string.IsNullOrWhiteSpace(dn.Consecutive)
            ? await _consecutive.NextAsync("ND", ct)
            : dn.Consecutive;

        var doc = await _renderer.RenderDebitNoteAsync(debitNoteId, ct);

        var xmlKey = $"debit_notes/{debitNoteId}/{doc.XmlFileName}";
        var pdfKey = $"debit_notes/{debitNoteId}/{doc.PdfFileName}";

        await _storage.PutAsync(xmlKey, doc.XmlContentType, doc.XmlBytes, ct);
        await _storage.PutAsync(pdfKey, doc.PdfContentType, doc.PdfBytes, ct);

        var haciendaUp = await _health.IsHaciendaUpAsync(ct);

        if (!haciendaUp)
        {
            dn.TaxKey = taxKey;
            dn.Consecutive = consecutive;
            dn.XmlS3Key = xmlKey;
            dn.PdfS3Key = pdfKey;
            dn.ActivateContingency(DateTime.UtcNow);

            await _db.SaveChangesAsync(ct);

            return new EInvoicingResult(
                true,
                "Contingency",
                dn.TaxStatus,
                taxKey,
                consecutive,
                pdfKey,
                xmlKey,
                "Hacienda down."
            );
        }

        var submit = await _hacienda.SubmitAsync(
            new HaciendaSubmitRequest(
                "ND",
                taxKey,
                consecutive,
                Convert.ToBase64String(doc.XmlBytes)
            ),
            ct
        );

        if (!submit.IsSuccess || submit.Status == "Rejected")
        {
            dn.TaxStatus = "Rejected";
            dn.RaiseUpdated();
            await _db.SaveChangesAsync(ct);

            return new EInvoicingResult(
                false,
                "Normal",
                dn.TaxStatus,
                taxKey,
                consecutive,
                pdfKey,
                xmlKey,
                submit.Message
            );
        }

        dn.MarkEmitted(taxKey, consecutive, pdfKey, xmlKey, DateTime.UtcNow);
        await _db.SaveChangesAsync(ct);

        return new EInvoicingResult(
            true,
            "Normal",
            dn.TaxStatus,
            taxKey,
            consecutive,
            pdfKey,
            xmlKey,
            submit.Message
        );
    }
}
