using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Application;

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);
    Task<Ticket?> FindByIdAsync(Guid ticketId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Ticket>> FindReadyTicketsAsync(DateTimeOffset now, CancellationToken cancellationToken);
    Task<IReadOnlyList<Ticket>> FindOpenScheduledTicketsDueAsync(DateTimeOffset now, CancellationToken cancellationToken);
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken);
}
