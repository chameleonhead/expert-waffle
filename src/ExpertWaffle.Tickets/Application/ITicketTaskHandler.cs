using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Application;

public interface ITicketTaskHandler
{
    string TaskType { get; }
    Task HandleAsync(TicketTaskContext context, CancellationToken cancellationToken);
}
