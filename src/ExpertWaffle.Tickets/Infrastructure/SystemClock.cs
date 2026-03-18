using ExpertWaffle.Tickets.Application;

namespace ExpertWaffle.Tickets.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
