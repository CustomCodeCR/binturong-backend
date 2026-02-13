using Application.Abstractions.EInvoicing;

namespace Infrastructure.EInvoicing;

public sealed class SimpleConsecutiveGenerator : IConsecutiveGenerator
{
    private static long _seq = DateTime.UtcNow.Ticks;

    public Task<string> NextAsync(string documentType, CancellationToken ct)
    {
        var n = Interlocked.Increment(ref _seq);
        var prefix = documentType switch
        {
            "INV" => "FE",
            "NC" => "NC",
            "ND" => "ND",
            _ => "DOC",
        };

        return Task.FromResult($"{prefix}-{n}");
    }
}
