using ExpertWaffle.Tickets.Application;

namespace ExpertWaffle.Tickets.Scheduling;

public sealed class TicketSchedulerService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IClock _clock;

    public TicketSchedulerService(
        ITicketRepository ticketRepository,
        IClock clock)
    {
        _ticketRepository = ticketRepository;
        _clock = clock;
    }

    public async Task PromoteReadyTicketsAsync(CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var tickets = await _ticketRepository.FindOpenScheduledTicketsDueAsync(now, cancellationToken);

        foreach (var ticket in tickets)
        {
            ticket.MarkReady(now);
            await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        }
    }
}
