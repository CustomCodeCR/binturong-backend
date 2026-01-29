namespace Api.Endpoints.UnitsOfMeasure;

public sealed record CreateUnitOfMeasureRequest(string Code, string Name, bool IsActive = true);

public sealed record UpdateUnitOfMeasureRequest(string Code, string Name, bool IsActive);
