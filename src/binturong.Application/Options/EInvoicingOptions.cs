namespace Application.Options;

public sealed class EInvoicingOptions
{
    public bool UseFakeHacienda { get; set; } = true;
    public bool ForceContingency { get; set; } = false;

    // For LocalDocumentStorage
    public string StorageRootPath { get; set; } = "storage/einvoicing";

    // For HaciendaHttpClient (future)
    public string HaciendaBaseUrl { get; set; } = "https://api.hacienda.example";
    public string HaciendaApiKey { get; set; } = "change-me";
}
