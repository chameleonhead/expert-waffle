using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Scheduling;

public interface IScheduleTrigger
{
    bool ShouldFire(DateTimeOffset now, Ticket ticket);
}
