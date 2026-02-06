using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure.Messaging;

internal sealed class CommandBus : ICommandBus
{
    private readonly IServiceProvider _provider;

    public CommandBus(IServiceProvider provider) => _provider = provider;

    public Task<Result> Send<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand
    {
        var handler = _provider.GetRequiredService<ICommandHandler<TCommand>>();
        return handler.Handle(command, ct);
    }

    public Task<Result<TResult>> Send<TCommand, TResult>(
        TCommand command,
        CancellationToken ct = default
    )
        where TCommand : ICommand<TResult>
    {
        var handler = _provider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return handler.Handle(command, ct);
    }
}
