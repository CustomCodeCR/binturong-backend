namespace Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string CreateToken(Guid userId);
}
