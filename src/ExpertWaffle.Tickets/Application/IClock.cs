namespace ExpertWaffle.Tickets.Application;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
