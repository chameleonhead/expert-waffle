using ExpertWaffle.Tickets.Application;
using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Infrastructure;

public sealed class InMemoryTicketExecutionRepository : ITicketExecutionRepository
{
    private readonly Dictionary<Guid, TicketExecution> _store = new();
    private readonly object _lock = new();

    public Task AddAsync(TicketExecution execution, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _store.Add(execution.Id, execution);
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TicketExecution execution, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _store[execution.Id] = execution;
        }
        return Task.CompletedTask;
    }
}
