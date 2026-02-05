namespace Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    string Generate(
        Guid userId,
        string username,
        string email,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> scopes
    );
}
