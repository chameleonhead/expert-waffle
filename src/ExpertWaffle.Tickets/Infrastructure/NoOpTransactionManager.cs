using ExpertWaffle.Tickets.Application;

namespace ExpertWaffle.Tickets.Infrastructure;

public sealed class NoOpTransactionManager : ITransactionManager
{
    public Task<ITransactionScope> BeginAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<ITransactionScope>(new NoOpTransactionScope());
    }
}

internal sealed class NoOpTransactionScope : ITransactionScope
{
    public Task CommitAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
