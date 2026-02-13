using Application.Abstractions.EInvoicing;

namespace Infrastructure.EInvoicing;

public sealed class FakeHaciendaClient : IHaciendaClient
{
    public Task<HaciendaSubmitResult> SubmitAsync(HaciendaSubmitRequest req, CancellationToken ct)
    {
        // Fake: always accepted
        return Task.FromResult(new HaciendaSubmitResult(true, "Accepted", null));
    }
}
