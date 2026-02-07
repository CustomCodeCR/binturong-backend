namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string QuotesRead = "quotes.read";
    public const string QuotesCreate = "quotes.create";
    public const string QuotesUpdate = "quotes.update";
    public const string QuotesDelete = "quotes.delete";

    public const string QuotesSend = "quotes.send";
    public const string QuotesAccept = "quotes.accept";
    public const string QuotesReject = "quotes.reject";
    public const string QuotesExpire = "quotes.expire";

    public const string QuotesDetailsAdd = "quotes.details.add";
}
