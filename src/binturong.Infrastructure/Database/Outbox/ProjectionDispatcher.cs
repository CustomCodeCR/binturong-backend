using System.Collections.Concurrent;
using System.Reflection;
using Application.Abstractions.Projections;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure.Database.Outbox;

internal sealed class ProjectionDispatcher : IProjectionDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    private static readonly ConcurrentDictionary<Type, Type> ProjectorEnumerableTypeCache = new();
    private static readonly ConcurrentDictionary<
        (Type ProjectorType, Type EventType),
        MethodInfo
    > MethodCache = new();

    public ProjectionDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        using var scope = _serviceProvider.CreateScope();

        var eventType = domainEvent.GetType();

        // Resolve IEnumerable<IProjector<TEvent>>
        var enumerableServiceType = ProjectorEnumerableTypeCache.GetOrAdd(
            eventType,
            t => typeof(IEnumerable<>).MakeGenericType(typeof(IProjector<>).MakeGenericType(t))
        );

        var projectorsObj = scope.ServiceProvider.GetService(enumerableServiceType);

        if (projectorsObj is not System.Collections.IEnumerable projectors)
        {
            throw new InvalidOperationException(
                $"No projectors registered for event '{eventType.FullName}'."
            );
        }

        var any = false;

        foreach (var projector in projectors)
        {
            if (projector is null)
                continue;

            any = true;

            var projectorType = projector.GetType();

            // Find exact: Task ProjectAsync(<eventType>, CancellationToken)
            var method = MethodCache.GetOrAdd(
                (projectorType, eventType),
                key =>
                {
                    var (pt, et) = key;

                    var mi = pt.GetMethod(
                        "ProjectAsync",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        binder: null,
                        types: new[] { et, typeof(CancellationToken) },
                        modifiers: null
                    );

                    if (mi is null)
                    {
                        throw new InvalidOperationException(
                            $"Projector '{pt.FullName}' does not implement ProjectAsync({et.FullName}, CancellationToken)."
                        );
                    }

                    if (!typeof(Task).IsAssignableFrom(mi.ReturnType))
                    {
                        throw new InvalidOperationException(
                            $"Projector '{pt.FullName}' ProjectAsync must return Task."
                        );
                    }

                    return mi;
                }
            );

            var taskObj = method.Invoke(projector, new object[] { domainEvent, cancellationToken });

            if (taskObj is not Task task)
            {
                throw new InvalidOperationException(
                    $"Projector '{projectorType.FullName}' ProjectAsync did not return a Task."
                );
            }

            await task.ConfigureAwait(false);
        }

        if (!any)
        {
            throw new InvalidOperationException(
                $"No projectors registered for event '{eventType.FullName}'."
            );
        }
    }
}
