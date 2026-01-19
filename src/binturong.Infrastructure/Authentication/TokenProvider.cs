using Application.Abstractions.Authentication;

namespace Infrastructure.Authentication;

internal sealed class TokenProvider : ITokenProvider
{
    public string CreateToken(Guid userId) => string.Empty; // TODO JWT real
}
