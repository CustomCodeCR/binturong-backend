namespace Application.Abstractions.EInvoicing;

public interface IElectronicDocumentRenderer
{
    Task<RenderedElectronicDocument> RenderInvoiceAsync(Guid invoiceId, CancellationToken ct);
    Task<RenderedElectronicDocument> RenderCreditNoteAsync(Guid creditNoteId, CancellationToken ct);
    Task<RenderedElectronicDocument> RenderDebitNoteAsync(Guid debitNoteId, CancellationToken ct);
}

public sealed record RenderedElectronicDocument(
    string XmlFileName,
    string PdfFileName,
    string XmlContentType,
    string PdfContentType,
    byte[] XmlBytes,
    byte[] PdfBytes
);

public interface IDocumentStorage
{
    Task<StoredDocument> PutAsync(
        string key,
        string contentType,
        byte[] bytes,
        CancellationToken ct
    );
}

public sealed record StoredDocument(string Key, long Size);

public interface IHaciendaClient
{
    Task<HaciendaSubmitResult> SubmitAsync(HaciendaSubmitRequest req, CancellationToken ct);
}

public sealed record HaciendaSubmitRequest(
    string DocumentType, // "INV" | "NC" | "ND"
    string TaxKey,
    string Consecutive,
    string XmlBase64
);

public sealed record HaciendaSubmitResult(
    bool IsSuccess,
    string Status, // "Accepted" | "Rejected" | "Processing"
    string? Message
);

public interface IConsecutiveGenerator
{
    Task<string> NextAsync(string documentType, CancellationToken ct);
}

public interface ITaxKeyGenerator
{
    Task<string> GenerateAsync(string documentType, CancellationToken ct);
}

public interface IExternalServiceHealth
{
    Task<bool> IsHaciendaUpAsync(CancellationToken ct);
}

public interface IElectronicInvoicingService
{
    Task<EInvoicingResult> EmitInvoiceAsync(Guid invoiceId, CancellationToken ct);
    Task<EInvoicingResult> EmitCreditNoteAsync(Guid creditNoteId, CancellationToken ct);
    Task<EInvoicingResult> EmitDebitNoteAsync(Guid debitNoteId, CancellationToken ct);
}

public sealed record EInvoicingResult(
    bool IsSuccess,
    string Mode, // "Normal" | "Contingency"
    string TaxStatus,
    string TaxKey,
    string Consecutive,
    string PdfKey,
    string XmlKey,
    string? Message
);
