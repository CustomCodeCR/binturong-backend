using Application.Abstractions.Documents;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace Infrastructure.Documents;

public sealed class DinkToPdfGenerator : IPdfGenerator
{
    private readonly IConverter _converter;

    public DinkToPdfGenerator(IConverter converter)
    {
        _converter = converter;
    }

    public Task<byte[]> RenderHtmlToPdfAsync(string html, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var document = new HtmlToPdfDocument
        {
            GlobalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings
                {
                    Top = 10,
                    Bottom = 10,
                    Left = 10,
                    Right = 10,
                },
                DocumentTitle = "Document",
            },
            Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = html,
                    PagesCount = true,
                    WebSettings = new WebSettings
                    {
                        DefaultEncoding = "utf-8",
                        LoadImages = true,
                        EnableJavascript = false,
                        PrintMediaType = true,
                    },
                },
            },
        };

        var bytes = _converter.Convert(document);
        return Task.FromResult(bytes);
    }
}
