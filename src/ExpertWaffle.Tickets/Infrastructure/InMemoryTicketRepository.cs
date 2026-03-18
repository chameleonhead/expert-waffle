using ExpertWaffle.Tickets.Application;
using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Infrastructure;

public sealed class InMemoryTicketRepository : ITicketRepository
{
    private readonly Dictionary<Guid, Ticket> _store = new();
    private readonly object _lock = new();

    public Task AddAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _store.Add(ticket.Id, ticket);
        }
        return Task.CompletedTask;
    }

    public Task<Ticket?> FindByIdAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _store.TryGetValue(ticketId, out var ticket);
            return Task.FromResult(ticket);
        }
    }

    public Task<IReadOnlyList<Ticket>> FindReadyTicketsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            var result = _store.Values
                .Where(x => x.Status == TicketStatus.Ready)
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.CreatedAt)
                .ToList()
                .AsReadOnly();

            return Task.FromResult((IReadOnlyList<Ticket>)result);
        }
    }

    public Task<IReadOnlyList<Ticket>> FindOpenScheduledTicketsDueAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            var result = _store.Values
                .Where(x => x.Status == TicketStatus.Open
                         && x.ScheduledAt.HasValue
                         && x.ScheduledAt.Value <= now)
                .OrderBy(x => x.ScheduledAt)
                .ThenByDescending(x => x.Priority)
                .ToList()
                .AsReadOnly();

            return Task.FromResult((IReadOnlyList<Ticket>)result);
        }
    }

    public Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _store[ticket.Id] = ticket;
        }
        return Task.CompletedTask;
    }
}
