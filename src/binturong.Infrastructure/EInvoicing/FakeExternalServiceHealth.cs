using Application.Abstractions.EInvoicing;

namespace Infrastructure.EInvoicing;

public sealed class FakeExternalServiceHealth : IExternalServiceHealth
{
    private readonly bool _isUp;

    public FakeExternalServiceHealth(bool isUp = true) => _isUp = isUp;

    public Task<bool> IsHaciendaUpAsync(CancellationToken ct) => Task.FromResult(_isUp);
}
