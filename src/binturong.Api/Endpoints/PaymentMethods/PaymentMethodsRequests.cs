namespace Api.Endpoints.PaymentMethods;

public sealed record CreatePaymentMethodRequest(string Code, string Description, bool IsActive);

public sealed record UpdatePaymentMethodRequest(string Code, string Description, bool IsActive);
