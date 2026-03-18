using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Application;

public sealed class TicketQueryService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IClock _clock;

    public TicketQueryService(
        ITicketRepository ticketRepository,
        IClock clock)
    {
        _ticketRepository = ticketRepository;
        _clock = clock;
    }

    public Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        return _ticketRepository.FindByIdAsync(ticketId, cancellationToken);
    }

    public Task<IReadOnlyList<Ticket>> GetReadyTicketsAsync(CancellationToken cancellationToken)
    {
        return _ticketRepository.FindReadyTicketsAsync(_clock.UtcNow, cancellationToken);
    }
}
