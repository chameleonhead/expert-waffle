using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Scheduling;

public sealed class ScheduledAtTrigger : IScheduleTrigger
{
    public bool ShouldFire(DateTimeOffset now, Ticket ticket)
    {
        return ticket.ScheduledAt.HasValue
            && ticket.Status == TicketStatus.Open
            && ticket.ScheduledAt.Value <= now;
    }
}
