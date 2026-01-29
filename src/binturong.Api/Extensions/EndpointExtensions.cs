using System.Reflection;
using Api.Endpoints;

namespace Api.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        var endpointTypes = assembly
            .DefinedTypes.Where(t =>
                !t.IsAbstract && !t.IsInterface && typeof(IEndpoint).IsAssignableFrom(t)
            )
            .ToList();

        foreach (var type in endpointTypes)
            services.AddSingleton(typeof(IEndpoint), type);

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.ServiceProvider.GetServices<IEndpoint>();
        foreach (var endpoint in endpoints)
            endpoint.MapEndpoint(app);

        return app;
    }
}
