using System.Net.Http.Json;
using Application.Abstractions.EInvoicing;

namespace Infrastructure.EInvoicing;

public sealed class HaciendaHttpClient : IHaciendaClient
{
    private readonly HttpClient _http;

    public HaciendaHttpClient(HttpClient http) => _http = http;

    public async Task<HaciendaSubmitResult> SubmitAsync(
        HaciendaSubmitRequest req,
        CancellationToken ct
    )
    {
        // Skeleton: adapt to real Hacienda API later
        var response = await _http.PostAsJsonAsync("/v1/documents/submit", req, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            return new HaciendaSubmitResult(false, "Rejected", body);
        }

        var payload = await response.Content.ReadFromJsonAsync<HaciendaSubmitResult>(
            cancellationToken: ct
        );
        return payload ?? new HaciendaSubmitResult(true, "Processing", "No payload returned");
    }
}
