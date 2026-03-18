namespace ExpertWaffle.Tickets.Application;

public interface ITransactionManager
{
    Task<ITransactionScope> BeginAsync(CancellationToken cancellationToken);
}

public interface ITransactionScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
    Task RollbackAsync(CancellationToken cancellationToken);
}
