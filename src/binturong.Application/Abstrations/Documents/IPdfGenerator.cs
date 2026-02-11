namespace Application.Abstractions.Documents;

public interface IPdfGenerator
{
    Task<byte[]> RenderHtmlToPdfAsync(string html, CancellationToken ct);
}
