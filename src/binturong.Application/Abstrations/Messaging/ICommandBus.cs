using SharedKernel;

namespace Application.Abstractions.Messaging;

public interface ICommandBus
{
    Task<Result> Send<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand;

    Task<Result<TResult>> Send<TCommand, TResult>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand<TResult>;
}
