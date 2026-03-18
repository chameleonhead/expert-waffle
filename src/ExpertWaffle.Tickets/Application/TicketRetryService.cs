using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Application;

public sealed class TicketRetryService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IClock _clock;

    public TicketRetryService(
        ITicketRepository ticketRepository,
        IClock clock)
    {
        _ticketRepository = ticketRepository;
        _clock = clock;
    }

    public async Task<bool> RetryAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.FindByIdAsync(ticketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket not found: {ticketId}");

        if (ticket.Status != TicketStatus.Failed)
            throw new InvalidOperationException($"Ticket is not in Failed state: {ticket.Status}");

        if (!ticket.CanRetry())
            return false;

        ticket.MarkReady(_clock.UtcNow);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        return true;
    }
}
