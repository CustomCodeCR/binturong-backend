using Application.Abstractions.EInvoicing;

namespace Infrastructure.EInvoicing;

public sealed class SimpleTaxKeyGenerator : ITaxKeyGenerator
{
    public Task<string> GenerateAsync(string documentType, CancellationToken ct)
    {
        // Placeholder key: replace later with CR rules for "clave"
        var key = $"{documentType}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
        return Task.FromResult(key);
    }
}
