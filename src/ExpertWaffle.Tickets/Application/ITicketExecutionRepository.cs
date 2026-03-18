using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Application;

public interface ITicketExecutionRepository
{
    Task AddAsync(TicketExecution execution, CancellationToken cancellationToken);
    Task UpdateAsync(TicketExecution execution, CancellationToken cancellationToken);
}
