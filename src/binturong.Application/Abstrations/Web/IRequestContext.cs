namespace Application.Abstractions.Web;

public interface IRequestContext
{
    string IpAddress { get; }
    string UserAgent { get; }
}
