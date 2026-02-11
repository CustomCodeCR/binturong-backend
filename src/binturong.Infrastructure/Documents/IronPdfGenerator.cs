using Application.Abstractions.Documents;

namespace Infrastructure.Documents;

public sealed class IronPdfGenerator : IPdfGenerator
{
    public Task<byte[]> RenderHtmlToPdfAsync(string html, CancellationToken ct)
    {
        // IronPDF is sync; wrap to keep interface async-friendly
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        return Task.FromResult(pdf.BinaryData);
    }
}
