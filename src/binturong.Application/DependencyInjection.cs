using Application.Abstractions.Behaviors;
using Application.Abstractions.Messaging;
using Application.Abstractions.Projections;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // =========================
        // Handlers
        // =========================
        services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        // =========================
        // Domain event handlers
        // =========================
        services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        // =========================
        // Projectors
        // =========================
        services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(c => c.AssignableTo(typeof(IProjector<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        // =========================
        // Validators
        // =========================
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // =========================
        // Decorators (safe)
        // =========================

        // Queries
        services.TryDecorateOpenGeneric(
            typeof(IQueryHandler<,>),
            typeof(LoggingDecorator.QueryHandler<,>)
        );

        // Commands (validation then logging)
        services.TryDecorateOpenGeneric(
            typeof(ICommandHandler<,>),
            typeof(ValidationDecorator.CommandHandler<,>)
        );
        services.TryDecorateOpenGeneric(
            typeof(ICommandHandler<>),
            typeof(ValidationDecorator.CommandBaseHandler<>)
        );

        services.TryDecorateOpenGeneric(
            typeof(ICommandHandler<,>),
            typeof(LoggingDecorator.CommandHandler<,>)
        );
        services.TryDecorateOpenGeneric(
            typeof(ICommandHandler<>),
            typeof(LoggingDecorator.CommandBaseHandler<>)
        );

        return services;
    }

    /// <summary>
    /// Decorates an open generic service type ONLY if there are closed registrations present.
    /// Also avoids crashing if Scrutor can't decorate (common when you have 0 handlers yet).
    /// </summary>
    private static IServiceCollection TryDecorateOpenGeneric(
        this IServiceCollection services,
        Type openGenericServiceType,
        Type openGenericDecoratorType
    )
    {
        // Must be open generic definitions
        if (!openGenericServiceType.IsGenericTypeDefinition)
            throw new ArgumentException(
                "Service type must be open generic.",
                nameof(openGenericServiceType)
            );

        if (!openGenericDecoratorType.IsGenericTypeDefinition)
            throw new ArgumentException(
                "Decorator type must be open generic.",
                nameof(openGenericDecoratorType)
            );

        // Check for any CLOSED registrations like IQueryHandler<Foo, Bar>
        var hasClosedRegistrations = services.Any(sd =>
            sd.ServiceType.IsGenericType
            && sd.ServiceType.GetGenericTypeDefinition() == openGenericServiceType
        );

        if (!hasClosedRegistrations)
            return services;

        try
        {
            services.Decorate(openGenericServiceType, openGenericDecoratorType);
        }
        catch (Scrutor.DecorationException)
        {
            // If Scrutor still can't decorate (edge cases), skip to avoid killing the app
            // You can log this later if you want.
        }

        return services;
    }
}
